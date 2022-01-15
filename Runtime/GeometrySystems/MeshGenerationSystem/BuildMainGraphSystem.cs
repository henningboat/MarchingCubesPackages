using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal class BuildMainGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;

            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.Persistent);
        }

        public JobHandle Update(JobHandle jobHandle, List<GeometryInstructionListBuffers> geometryGraphBuffersList)
        {
            //todo placeholder
            jobHandle.Complete();

            _allGeometryInstructionsList.Clear();

            SerializableGUID outputLayerID = default;

            _geometryPerLayer = new();
            foreach (var geometryInstructionList in geometryGraphBuffersList)
            {
                if (!_geometryPerLayer.ContainsKey(geometryInstructionList.TargetLayer.ID))
                    _geometryPerLayer.Add(geometryInstructionList.TargetLayer.ID,
                        new List<GeometryInstructionListBuffers>());

                _geometryPerLayer[geometryInstructionList.TargetLayer.ID].Add(geometryInstructionList);
            }

            var outputLayerInstructionLists = _geometryPerLayer[outputLayerID];
            foreach (var outputLayerInstructionList in outputLayerInstructionLists)
                AddInstructionListToMainGraph(outputLayerInstructionList, 0);

            MainRenderGraph = _allGeometryInstructionsList.AsArray();

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
                    var childLayerContent = _geometryPerLayer[instruction.SourceLayerID];
                    foreach (var childInstructionList in childLayerContent)
                        AddInstructionListToMainGraph(childInstructionList, instruction.CombinerDepth);
                }
            }
        }

        public void Dispose()
        {
            _allGeometryInstructionsList.Dispose();
        }

        public NativeArray<GeometryInstruction> MainRenderGraph { get; private set; } = default;
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