using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryChunkGPUIndices : IComponentData
    {
        public int DistanceFieldBufferOffset;
    }
}