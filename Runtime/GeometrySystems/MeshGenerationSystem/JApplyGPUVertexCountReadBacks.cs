using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    [BurstCompile]
    public struct JApplyGPUVertexCountReadBacks : IJobParallelFor
    {
        public GeometryFieldData GeometryFieldData;
        [ReadOnly] public NativeArray<int> VertexCountPerSubChunkReadback;
        [ReadOnly] public NativeArray<int> ReadbackTimeStampPerCluster;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> VertexCountPerSubChunk;

        public void Execute(int clusterIndex)
        {
            var timeStampOfReadback = ReadbackTimeStampPerCluster[clusterIndex];
            var cluster = GeometryFieldData.GetCluster(clusterIndex);

            for (var chunkIndex = 0; chunkIndex < Constants.chunksPerCluster; chunkIndex++)
            {
                var chunk = cluster.GetChunk(chunkIndex);
                if (chunk.Parameters.InstructionChangeTimeStamp <= timeStampOfReadback)
                    for (var i = 0; i < 8; i++)
                    {
                        var subChunkIndex = clusterIndex * Constants.subChunksPerCluster +
                                            chunkIndex * Constants.subChunksPerChunk + i;
                        VertexCountPerSubChunk[subChunkIndex] =
                            math.max(0, VertexCountPerSubChunkReadback[subChunkIndex]);
                    }
            }
        }
    }
}