using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Rendering
{
    public struct CSubChunkWithTrianglesIndex
    {
        public int3 ChunkPositionGS;
        public int SubChunkIndex;

        public CSubChunkWithTrianglesIndex(int3 chunkPositionGs, int subChunkIndex, bool hasData)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }
}