using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup
{
    public struct CClusterParameters
    {
        public bool needsIndexBufferUpdate;
        public BitArray512 WriteMask;
        public int vertexCount;
        public int lastVertexBufferChangeTimestamp;
        public int triangulationInstructionCount;
        public int3 PositionWS;
        public int ClusterIndex;
        public int subChunksWithTrianglesCount;
    }
}