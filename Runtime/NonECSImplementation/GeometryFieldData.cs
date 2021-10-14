using System;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace NonECSImplementation
{
    public class GeometryFieldData
    {
        public const int chunksPerCluster = 8*8*8;
        public const int chunkLength = 8;
        public const int subChunksPerChunk = 8;
        public const int subChunksPerCluster = chunksPerCluster*subChunksPerChunk;
        public const int clusterLength = chunkLength * 8;
        public const int clusterVolume = clusterLength * clusterLength * clusterLength;

        public int TotalVoxelCount { get; private set; }
        public int TotalChunkCount { get; private set; }
        public int TotalSubChunkCount { get; private set; }

        public NativeArray<PackedTerrainData> GeometryBuffer;
        public NativeArray<CClusterPosition> ClusterPositions;
        public NativeArray<DistanceFieldChunkData> DistanceFieldChunkDatas;
        public int3 GeometryClusterChunkCounts;
        public int ClusterCount { get; private set; }

        public void Dispose()
        {
            GeometryBuffer.Dispose();
            ClusterPositions.Dispose();
            DistanceFieldChunkDatas.Dispose();
        }

        public void Initialize(int3 clusterCounts)
        {
            GeometryClusterChunkCounts = clusterCounts;
            ClusterCount = clusterCounts.Volume();
            TotalVoxelCount = ClusterCount * clusterVolume;
            GeometryBuffer = new NativeArray<PackedTerrainData>(TotalVoxelCount, Allocator.Persistent);

            InitializeJob job = new InitializeJob() {geometryBuffer = GeometryBuffer};
            job.Execute();

            TotalChunkCount = ClusterCount * chunksPerCluster;
            TotalSubChunkCount = TotalChunkCount * subChunksPerChunk;

            ClusterPositions = new NativeArray<CClusterPosition>(ClusterCount, Allocator.Persistent);
            ClusterParameters = new NativeArray<CClusterParameters>(ClusterCount, Allocator.Persistent);
            DistanceFieldChunkDatas = new NativeArray<DistanceFieldChunkData>(ClusterCount, Allocator.Persistent);

            var clusterIndex = 0;

            for (var x = 0; x < clusterCounts.x; x++)
            for (var y = 0; y < clusterCounts.y; y++)
            for (var z = 0; z < clusterCounts.z; z++)
            {
                ClusterPositions[clusterIndex] = new CClusterPosition() {ClusterIndex = clusterIndex, PositionGS = new int3(x, y, z)};
            }
        }

        public NativeArray<CClusterParameters> ClusterParameters { get; set; }

        [BurstCompile]
        private struct InitializeJob:IJob
        {
            public NativeArray<PackedTerrainData> geometryBuffer;
            public void Execute()
            {
                for (int i = 0; i < geometryBuffer.Length; i++)
                {
                    geometryBuffer[0] = new PackedTerrainData() {SurfaceDistance = -10};
                }
            }
        }

        public void AddRandomVoxel()
        {
            GeometryBuffer[Random.Range(0, GeometryBuffer.Length)] = new PackedTerrainData() {SurfaceDistance = 1};
        }
    }
}
