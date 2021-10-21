using henningboat.CubeMarching.Utils;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup
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