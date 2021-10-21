using Unity.Mathematics;
using Utils;

namespace Rendering
{
    public struct CClusterParameters
    {
        public bool needsIndexBufferUpdate;
        public BitArray512 WriteMask;
        public int vertexCount;
        public int lastVertexBufferChangeTimestamp;
        public int triangulationInstructionCount;
        public int3 PositionGS;
        public int ClusterIndex;
        public int subChunksWithTrianglesCount;
    }
}