﻿using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using henningboat.CubeMarching.Runtime.NewDistanceFieldResolverPrototype;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.Output.GeometryFieldMeshRendererSystem
{
    public class GeometryLayerHandler : IDisposable
    {
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;
        private List<SerializableGUID> _allLayerIDs;
        private List<GeometryLayer> _allLayers;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;
        private List<GeometryLayer> _storedLayers;
        public NativeList<int> ChunksUpdatedThisFrame;

        public GeometryLayerHandler(int3 clusterCounts, GeometryLayer geometryLayer)
        {
            GeometryFieldData = new GeometryFieldData(clusterCounts, geometryLayer, Allocator.Persistent);
            ChunksUpdatedThisFrame = new NativeList<int>(GeometryFieldData.TotalChunkCount, Allocator.Persistent);
        }

        public GeometryFieldData GeometryFieldData { get; }

        public int3 ClusterCounts => GeometryFieldData.ClusterCounts;
        public GeometryLayer GeometryLayer => GeometryFieldData.GeometryLayer;

        public void Dispose()
        {
            GeometryFieldData.Dispose().Complete();
            ChunksUpdatedThisFrame.Dispose();
        }

        public JobHandle Update(JobHandle jobHandle,
            Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> geometryPerLayer,
            List<GeometryLayer> storedGeometryLayers,
            List<GeometryLayer> allLayers, List<GeometryLayerHandler> allLayerHandlers, bool forceClear,
            BinaryDataStorage binaryDataStorage)
        {
            _allLayers = allLayers;
            _storedLayers = storedGeometryLayers;
            _allLayerIDs = geometryPerLayer.Keys.ToList();

            ChunksUpdatedThisFrame.Clear();

            //todo placeholder
            jobHandle.Complete();

            _geometryPerLayer = geometryPerLayer;

            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.TempJob);

            var outputLayerInstructionLists = _geometryPerLayer[GeometryFieldData.GeometryLayer.ID];


            if (outputLayerInstructionLists == null) return jobHandle;

            foreach (var outputLayerInstructionList in outputLayerInstructionLists)
                AddInstructionListToMainGraph(outputLayerInstructionList, 0);

            var geometryInstructions = _allGeometryInstructionsList.AsArray();

            var readbackCollection = new DistanceDataReadbackCollection(binaryDataStorage);

            for (var i = 0; i < readbackCollection.Capacity; i++)
                if (i < allLayerHandlers.Count)
                    readbackCollection[i] = allLayerHandlers[i].GeometryFieldData;
                else
                    readbackCollection[i] = new GeometryFieldData(0, default, Allocator.TempJob);

            var geometryLayerIndex = GetLayerIndex(GeometryFieldData.GeometryLayer);
            // var prepassJob =
            //     new JExecuteDistanceFieldPrepass(readbackCollection, geometryLayerIndex, geometryInstructions);
            // jobHandle = prepassJob.Schedule(GeometryFieldData.ClusterCount, 1, jobHandle);

            var calculateDistanceFieldJob = new JCalculateDistanceField(geometryLayerIndex,
                geometryInstructions, readbackCollection);
            jobHandle = calculateDistanceFieldJob.Schedule(GeometryFieldData.TotalChunkCount, 1, jobHandle);

            for (var i = 0; i < readbackCollection.Capacity; i++)
                if (i >= allLayerHandlers.Count)
                    jobHandle = readbackCollection[i].Dispose(jobHandle);



            var getChunksWithModificationsJob = new JGetChunksWithModifications
            {
                GeometryField = GeometryFieldData,
                ChunksWithModifiedIndices = ChunksUpdatedThisFrame.AsParallelWriter()
            };

            jobHandle = getChunksWithModificationsJob.Schedule(GeometryFieldData.TotalChunkCount, 256, jobHandle);

            //todo remove this
            jobHandle.Complete();
            
            //todo placeholder test
            JRecursivelyResolveDistanceField job = new JRecursivelyResolveDistanceField
            {
                GeometryFieldBuffer = GeometryFieldData.GeometryBuffer,
                Instructions = _allGeometryInstructionsList,
            };
            
            jobHandle = job.Schedule(jobHandle);
            
            jobHandle = _allGeometryInstructionsList.Dispose(jobHandle);
            
            //todo remove this
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
                        geometryLayer => geometryLayer.ID == instruction.ReferenceGUID);
                    if (sourceLayer.ID.Valid)
                    {
                        if (sourceLayer.Stored)
                        {
                            _allGeometryInstructionsList.Add(CreateLayerCopyInstruction(sourceLayer,
                                instruction.CombinerDepth));
                        }
                        else
                        {
                            if (_geometryPerLayer.TryGetValue(instruction.ReferenceGUID, out var childLayerContent))
                                foreach (var childInstructionList in childLayerContent)
                                    AddInstructionListToMainGraph(childInstructionList, instruction.CombinerDepth);
                        }
                    }
                }
            }
        }

        private GeometryInstruction CreateLayerCopyInstruction(GeometryLayer sourceLayer, int combinerDepth)
        {
            var layerIndex = GetLayerIndex(sourceLayer);
            return new GeometryInstruction
            {
                CombinerDepth = combinerDepth, GeometryInstructionSubType = layerIndex,
                GeometryInstructionType = GeometryInstructionType.CopyLayer
            };
        }

        private int GetLayerIndex(GeometryLayer sourceLayer)
        {
            return _storedLayers.FindIndex(layer => layer.ID == sourceLayer.ID);
        }
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