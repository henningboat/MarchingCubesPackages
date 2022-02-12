using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup
{
    public struct GeometryFieldData
    {
        public GeometryLayer GeometryLayer { get; }
        public int TotalVoxelCount { get; }
        public int TotalChunkCount { get; }
        public int TotalSubChunkCount { get; }
        public readonly int3 GeometryClusterChunkCounts;
        public int ClusterCount { get; }

        [NativeDisableParallelForRestriction] public  NativeArray<PackedDistanceFieldData> GeometryBuffer;
        [NativeDisableParallelForRestriction] public  NativeArray<GeometryChunkParameters> DistanceFieldChunkDatas;
        [NativeDisableParallelForRestriction] public  NativeArray<CClusterParameters> ClusterParameters;

        public JobHandle Dispose(JobHandle jobHandle=default)
        {
            if (GeometryBuffer.IsCreated) jobHandle=GeometryBuffer.Dispose(jobHandle);
            if (ClusterParameters.IsCreated)  jobHandle=ClusterParameters.Dispose(jobHandle);
            if (DistanceFieldChunkDatas.IsCreated)  jobHandle=DistanceFieldChunkDatas.Dispose(jobHandle);
            return jobHandle;
        }

        public GeometryFieldData(int3 clusterCounts, GeometryLayer layer, Allocator persistent)
        {
            if (layer.Stored == false&& math.any(clusterCounts>0)) throw new Exception("Only stored layers can have GeometryFieldData instances");

            GeometryLayer = layer;

            ClusterCounts = clusterCounts;
            GeometryClusterChunkCounts = clusterCounts;
            ClusterCount = clusterCounts.Volume();
            TotalVoxelCount = ClusterCount * Constants.clusterVolume;
            GeometryBuffer = new NativeArray<PackedDistanceFieldData>(TotalVoxelCount/Constants.PackedCapacity, persistent);

            TotalChunkCount = ClusterCount * Constants.chunksPerCluster;
            TotalSubChunkCount = TotalChunkCount * Constants.subChunksPerChunk;

            ClusterParameters = new NativeArray<CClusterParameters>(ClusterCount, persistent);
            DistanceFieldChunkDatas = new NativeArray<GeometryChunkParameters>(TotalChunkCount, persistent);

            if (TotalChunkCount == 0)
            {
                return;
            }
            
            var initializeDistanceFIeld = new InitializeTerrainData {geometryBuffer = GeometryBuffer};
            var jobHandle = initializeDistanceFIeld.Schedule();

            
            var clusterIndex = 0;

            for (var x = 0; x < clusterCounts.x; x++)
            for (var y = 0; y < clusterCounts.y; y++)
            for (var z = 0; z < clusterCounts.z; z++)
            {
                ClusterParameters[clusterIndex] = new CClusterParameters
                    {ClusterIndex = clusterIndex, PositionWS = new int3(x, y, z) * Constants.clusterLength};
                clusterIndex++;
            }

            var initializeChunkData = new JInitializeChunkParameters
            {
                GeometryChunkParameters = DistanceFieldChunkDatas, GeometryClusterParameters = ClusterParameters
            };
            jobHandle = initializeChunkData.Schedule(jobHandle);
            jobHandle.Complete();
        }

        public const int chunkVolume = Constants.chunkLength * Constants.chunkLength * Constants.chunkLength;
        public int3 ClusterCounts { get; }

        [BurstCompile]
        private struct InitializeTerrainData : IJob
        {
            public NativeArray<PackedDistanceFieldData> geometryBuffer;

            public void Execute()
            {
                for (var i = 0; i < geometryBuffer.Length; i++)
                    geometryBuffer[i] = new PackedDistanceFieldData {SurfaceDistance = 10};
            }
        }

        [BurstCompile]
        private struct JInitializeChunkParameters : IJob
        {
            [ReadOnly] public NativeArray<CClusterParameters> GeometryClusterParameters;
            [WriteOnly] public NativeArray<GeometryChunkParameters> GeometryChunkParameters;

            public void Execute()
            {
                for (var clusterIndex = 0; clusterIndex < GeometryClusterParameters.Length; clusterIndex++)
                {
                    var clusterParameter = GeometryClusterParameters[clusterIndex];
                    for (var chunkIndex = 0; chunkIndex < Constants.chunksPerCluster; chunkIndex++)
                    {
                        var totalIndex = chunkIndex + clusterIndex * Constants.chunksPerCluster;

                        var positionWS =
                            Runtime.DistanceFieldGeneration.Utils.IndexToPositionWS(chunkIndex,
                                Constants.chunkLengthPerCluster) * Constants.chunkLength + clusterParameter.PositionWS;

                        var chunkParameters = new GeometryChunkParameters
                        {
                            IndexInCluster = chunkIndex,
                            PositionWS = positionWS
                        };
                        GeometryChunkParameters[totalIndex] = chunkParameters;
                    }
                }
            }
        }

        public GeometryCluster GetCluster(int clusterIndex)
        {
            var distanceField = GeometryBuffer.Slice(Constants.clusterVolume / Constants.PackedCapacity * clusterIndex,
                Constants.clusterVolume / Constants.PackedCapacity);
            var clusterParameters = ClusterParameters.Slice(clusterIndex, 1);
            var chunkParameters =
                DistanceFieldChunkDatas.Slice(clusterIndex * Constants.chunksPerCluster, Constants.chunksPerCluster);

            return new GeometryCluster(distanceField, clusterParameters, chunkParameters);
        }

        public GeometryChunk GetChunk(int chunkIndex)
        {
            var cluster = GetCluster(chunkIndex / Constants.chunksPerCluster);
            return cluster.GetChunk(chunkIndex % Constants.chunksPerCluster);
        }
    }
}