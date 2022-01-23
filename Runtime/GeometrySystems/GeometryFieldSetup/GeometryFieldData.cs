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
    public struct GeometryFieldReadbackCollection
    {
        private GeometryFieldData _data0;
        private GeometryFieldData _data1;
        private GeometryFieldData _data2;
        private GeometryFieldData _data3;
        private GeometryFieldData _data4;
        private GeometryFieldData _data5;
        private GeometryFieldData _data6;
        private GeometryFieldData _data7;

        public GeometryFieldData this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _data0;
                    case 1: return _data1;
                    case 2: return _data2;
                    case 3: return _data3;
                    case 4: return _data4;
                    case 5: return _data5;
                    case 6: return _data6;
                    case 7: return _data7;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        _data0 = value;
                        break;
                    case 1:
                        _data1 = value;
                        break;
                    case 2:
                        _data2 = value;
                        break;
                    case 3:
                        _data3 = value;
                        break;
                    case 4:
                        _data4 = value;
                        break;
                    case 5:
                        _data5 = value;
                        break;
                    case 6:
                        _data6 = value;
                        break;
                    case 7:
                        _data7 = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

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

        public void Dispose()
        {
            if (GeometryBuffer.IsCreated) GeometryBuffer.Dispose();
            if (ClusterParameters.IsCreated) ClusterParameters.Dispose();
            if (DistanceFieldChunkDatas.IsCreated) DistanceFieldChunkDatas.Dispose();
        }

        public GeometryFieldData(int3 clusterCounts, GeometryLayer layer)
        {
            if (layer.Stored == false) throw new Exception("Only stored layers can have GeometryFieldData instances");

            GeometryLayer = layer;

            ClusterCounts = clusterCounts;
            GeometryClusterChunkCounts = clusterCounts;
            ClusterCount = clusterCounts.Volume();
            TotalVoxelCount = ClusterCount * Constants.clusterVolume;
            GeometryBuffer = new NativeArray<PackedDistanceFieldData>(TotalVoxelCount, Allocator.Persistent);

            var initializeDistanceFIeld = new InitializeTerrainData {geometryBuffer = GeometryBuffer};
            var jobHandle = initializeDistanceFIeld.Schedule();

            TotalChunkCount = ClusterCount * Constants.chunksPerCluster;
            TotalSubChunkCount = TotalChunkCount * Constants.subChunksPerChunk;

            ClusterParameters = new NativeArray<CClusterParameters>(ClusterCount, Allocator.Persistent);
            DistanceFieldChunkDatas = new NativeArray<GeometryChunkParameters>(TotalChunkCount, Allocator.Persistent);

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