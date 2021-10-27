using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.TerrainChunkSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup
{
    public struct GeometryFieldData
    {
        public int TotalVoxelCount { get; private set; }
        public int TotalChunkCount { get; private set; }
        public int TotalSubChunkCount { get; private set; }

        [NativeDisableParallelForRestriction] public NativeArray<PackedDistanceFieldData> GeometryBuffer;
        [NativeDisableParallelForRestriction] public NativeArray<GeometryChunkParameters> DistanceFieldChunkDatas;
        [NativeDisableParallelForRestriction]  public NativeArray<CClusterParameters> ClusterParameters;
        public int3 GeometryClusterChunkCounts;
        public int ClusterCount { get; private set; }


        public void Dispose()
        {
            if (GeometryBuffer.IsCreated)
            {
                GeometryBuffer.Dispose();
            }

            if (ClusterParameters.IsCreated)
            {
                ClusterParameters.Dispose();
            }

            if (DistanceFieldChunkDatas.IsCreated)
            {
                DistanceFieldChunkDatas.Dispose();
            }
        }

        public void Initialize(int3 clusterCounts)
        {
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
            {
                for (var y = 0; y < clusterCounts.y; y++)
                {
                    for (var z = 0; z < clusterCounts.z; z++)
                    {
                        ClusterParameters[clusterIndex] = new CClusterParameters
                            {ClusterIndex = clusterIndex, PositionWS = new int3(x, y, z) * Constants.clusterLength};
                        clusterIndex++;
                    }
                }
            }

            var initializeChunkData = new JInitializeChunkParameters
            {
                GeometryChunkParameters = DistanceFieldChunkDatas, GeometryClusterParameters = ClusterParameters
            };
            jobHandle = initializeChunkData.Schedule(jobHandle);
            jobHandle.Complete();
        }

        public const int chunkVolume = Constants.chunkLength * Constants.chunkLength * Constants.chunkLength;
        public int3 ClusterCounts { get; private set; }

        [BurstCompile]
        private struct InitializeTerrainData : IJob
        {
            public NativeArray<PackedDistanceFieldData> geometryBuffer;

            public void Execute()
            {
                for (var i = 0; i < geometryBuffer.Length; i++) geometryBuffer[i] = new PackedDistanceFieldData {SurfaceDistance = 10};
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
                            TerrainChunkEntitySystem.Utils.IndexToPositionWS(chunkIndex,
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
            var distanceField = GeometryBuffer.Slice((Constants.clusterVolume / Constants.PackedCapacity) * clusterIndex, (Constants.clusterVolume / Constants.PackedCapacity));
            var clusterParameters = ClusterParameters.Slice(clusterIndex,1);
            var chunkParameters = DistanceFieldChunkDatas.Slice(clusterIndex * Constants.chunksPerCluster, Constants.chunksPerCluster);
            
            return new GeometryCluster(distanceField, clusterParameters, chunkParameters);
        }
    }
}