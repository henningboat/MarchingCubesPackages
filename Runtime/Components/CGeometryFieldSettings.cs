using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryFieldSettings : IComponentData
    {
        public int3 ClusterCounts;
    }
}