using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryCluster:IComponentData
    {
        public int3 PositionWS;
    }
}