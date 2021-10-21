
using Unity.Mathematics;

namespace henningboat.CubeMarching.Rendering
{
    public struct CTriangulationInstruction
    {
        public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public CTriangulationInstruction(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }
}