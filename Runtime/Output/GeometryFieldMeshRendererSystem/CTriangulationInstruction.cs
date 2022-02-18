using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Rendering
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