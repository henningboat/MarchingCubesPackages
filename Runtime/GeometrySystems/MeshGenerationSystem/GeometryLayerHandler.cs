using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal class GeometryLayerHandler:IDisposable
    {
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;
        private List<GeometryLayer> _allLayers;

        public GeometryFieldData GeometryFieldData { get; }

        public GeometryLayerHandler(int3 clusterCounts, GeometryLayer geometryLayer)
        {
            GeometryFieldData = new GeometryFieldData(clusterCounts, geometryLayer);
            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.Persistent);
        }

        public unsafe JobHandle Update(JobHandle jobHandle,
            Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> geometryPerLayer,
            List<GeometryLayer> allLayers, GeometryLayerHandler[] allLayerHandlers)
        {
            _allLayers = allLayers;
            //todo placeholder
            jobHandle.Complete();

            _geometryPerLayer = geometryPerLayer;
            
            _allGeometryInstructionsList.Clear();

            var outputLayerInstructionLists = _geometryPerLayer[GeometryFieldData.GeometryLayer.ID];


            if (outputLayerInstructionLists == null)
            {
                return jobHandle;
            }
            
            _allGeometryInstructionsList.Add(CreateLayerCopyInstruction(GeometryLayer, 0));
            
            foreach (var outputLayerInstructionList in outputLayerInstructionLists)
                AddInstructionListToMainGraph(outputLayerInstructionList, 0);

            var geometryInstructions = _allGeometryInstructionsList.AsArray();

            var prepassJob = new JExecuteDistanceFieldPrepass(GeometryFieldData, geometryInstructions);
            jobHandle = prepassJob.Schedule(GeometryFieldData.ClusterCount, 1, jobHandle);
            
            jobHandle.Complete();

            DistanceDataReadbackCollection readbackCollection=new DistanceDataReadbackCollection();
            
            for (int i = 0; i < allLayers.Count; i++)
            {
                var geometryBuffer = allLayerHandlers[i].GeometryFieldData.GeometryBuffer;
                readbackCollection[i] = new UnsafeList<PackedDistanceFieldData>(
                    (PackedDistanceFieldData*) geometryBuffer.GetUnsafeReadOnlyPtr(), geometryBuffer.Length);
            }
            
            var calculateDistanceFieldJob = new JCalculateDistanceField(GeometryFieldData, geometryInstructions, readbackCollection);
            jobHandle = calculateDistanceFieldJob.Schedule(GeometryFieldData.TotalChunkCount, 1, jobHandle);
            
            jobHandle.Complete();
            
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
                    var sourceLayer = _allLayers.FirstOrDefault(
                        geometryLayer => geometryLayer.ID == instruction.SourceLayerID);
                    if (sourceLayer.ID.Valid)
                    {
                        if (sourceLayer.Stored)
                        {
                            _allGeometryInstructionsList.Add(CreateLayerCopyInstruction(sourceLayer,
                                instruction.CombinerDepth));
                        }
                        else
                        {
                            if (_geometryPerLayer.TryGetValue(instruction.SourceLayerID, out var childLayerContent))
                            {
                                foreach (var childInstructionList in childLayerContent)
                                    AddInstructionListToMainGraph(childInstructionList, instruction.CombinerDepth);
                            }
                        }
                    }
                }
            }
        }

        private GeometryInstruction CreateLayerCopyInstruction(GeometryLayer sourceLayer, int combinerDepth)
        {
            int layerIndex = _allLayers.FindIndex(layer => layer.ID == sourceLayer.ID);
            return new GeometryInstruction { CombinerDepth = combinerDepth, GeometryInstructionSubType = layerIndex, GeometryInstructionType = GeometryInstructionType.CopyLayer};
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