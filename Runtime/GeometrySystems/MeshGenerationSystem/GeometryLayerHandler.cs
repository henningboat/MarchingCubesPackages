using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal class GeometryLayerHandler:IDisposable
    {
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;

        public GeometryFieldData GeometryFieldData { get; }

        public GeometryLayerHandler(int3 clusterCounts, GeometryLayer geometryLayer)
        {
            GeometryFieldData = new GeometryFieldData(clusterCounts, geometryLayer);
            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.Persistent);
        }

        public JobHandle Update(JobHandle jobHandle,   Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> geometryPerLayer)
        {
            //todo placeholder
            jobHandle.Complete();

            _geometryPerLayer = geometryPerLayer;
            
            _allGeometryInstructionsList.Clear();

            //todo inline this again please
            var geometryFieldData = GeometryFieldData;
            
            var outputLayerInstructionLists = _geometryPerLayer[geometryFieldData.GeometryLayer.ID];


            if (outputLayerInstructionLists == null)
            {
                return jobHandle;
            }
            
            foreach (var outputLayerInstructionList in outputLayerInstructionLists)
                AddInstructionListToMainGraph(outputLayerInstructionList, 0);

            var geometryInstructions = _allGeometryInstructionsList.AsArray();
            
            
            var prepassJob = new JExecuteDistanceFieldPrepass(geometryFieldData, geometryInstructions);
            jobHandle = prepassJob.Schedule(geometryFieldData.ClusterCount, 1, jobHandle);

            geometryFieldData.GeometryLayer =
                new GeometryLayer("output", default, true, Input.GetKeyDown(KeyCode.Space));
            
            var calculateDistanceFieldJob = new JCalculateDistanceField(geometryFieldData, geometryInstructions);
            jobHandle = calculateDistanceFieldJob.Schedule(geometryFieldData.TotalChunkCount, 1, jobHandle);

            return jobHandle;
        }

        private void AddInstructionListToMainGraph(GeometryInstructionListBuffers outputLayerInstructionList,
            int combinerDepthOffset)
        {
            if (combinerDepthOffset > 50) throw new Exception("cyclic dependency detected while building main graph");

            for (var i = 0; i < outputLayerInstructionList.GeometryInstructions.Length; i++)
            {
                var instruction = outputLayerInstructionList.GeometryInstructions[i];
                if (instruction.GeometryInstructionType != GeometryInstructionType.CopyLayer)
                {
                    instruction.CombinerDepth += combinerDepthOffset;
                    _allGeometryInstructionsList.Add(instruction);
                }
                else
                {
                    if (_geometryPerLayer.TryGetValue(instruction.SourceLayerID, out var childLayerContent))
                        foreach (var childInstructionList in childLayerContent)
                            AddInstructionListToMainGraph(childInstructionList, instruction.CombinerDepth);
                }
            }
        }

        public void Dispose()
        {
            _allGeometryInstructionsList.Dispose();
            GeometryFieldData.Dispose();
        }
        
        public int3 ClusterCounts => GeometryFieldData.ClusterCounts;
        public GeometryLayer GeometryLayer => GeometryFieldData.GeometryLayer;
        
    }

    public static class NativeArrayExtensions
    {
        public static void CopyFrom<T>(this NativeArray<T> target, NativeArray<T> source, int targetStartIndex,
            int sourceStartIndex, int count) where T : struct
        {
            var targetSlice = target.Slice(targetStartIndex, count);
            var sourceSlice = source.Slice(sourceStartIndex, count);

            targetSlice.CopyFrom(sourceSlice);
        }
    }
}