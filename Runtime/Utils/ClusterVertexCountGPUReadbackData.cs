using Rendering;
using Unity.Collections;
using Unity.Jobs;

namespace Utils
{
    public struct ClusterVertexCountGPUReadbackData
    {
        public NativeArray<int> vertexCounts;
        public int clusterIndex;
        public int frameTimestamp;

        public ClusterVertexCountGPUReadbackData(AsyncReadbackUtility.ReadbackData readbackData)
        {
            vertexCounts = new NativeArray<int>(Constants.SubChunksInCluster, Allocator.TempJob);
            frameTimestamp = readbackData.frameTimestamp;    
            
            for (int i = 0; i < 4096; i++)
            {
                vertexCounts[i] = readbackData.vertexCount[i];
            }

            clusterIndex = readbackData.clusterIndex;
        }

        public void Dispose(JobHandle dependency)
        {
            vertexCounts.Dispose(dependency);
        }
    }
}