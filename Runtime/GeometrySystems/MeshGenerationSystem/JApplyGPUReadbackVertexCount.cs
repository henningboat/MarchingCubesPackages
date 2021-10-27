using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    [BurstCompatible]
    internal struct JApplyGPUReadbackVertexCount:IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> ClusterIndex;
        //[ReadOnly] public NativeArray<int> FrameTimestamp;
        [ReadOnly] public NativeArray<int> VertexCountReadbacks;
        
        [NativeDisableParallelForRestriction] public NativeArray<int> VertexCountPerSubChunk;

        public GeometryFieldData GeometryField;
        
        public void Execute(int index)
        {
            int clusterIndex = ClusterIndex[index];
            var cluster = GeometryField.GetCluster(clusterIndex);
            
            var vertexCountPerSubChunkInCluster = VertexCountPerSubChunk.Slice(cluster.Parameters.ClusterIndex * Constants.subChunksPerCluster,
                Constants.subChunksPerCluster);

            var readbacksForCluster =
                VertexCountReadbacks.Slice(clusterIndex * Constants.subChunksPerCluster, Constants.subChunksPerCluster);

            for (int i = 0; i < Constants.chunksPerCluster; i++)
            {
                var chunk = cluster.GetChunk(i);
                var parameters = chunk.Parameters;

                for (int j = 0; j < 8; j++)
                {
                    var subChunkInCluster = parameters.IndexInCluster*8+j;
                    vertexCountPerSubChunkInCluster[subChunkInCluster] = readbacksForCluster[subChunkInCluster];
                }
            }
        }
    }
}