using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryCluster:IComponentData
    {
        public float3 PositionWS;
        public Entity LayerEntity;
    }
}