using Unity.Collections;
using Unity.Entities;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    public struct GeometryLayerReadbackAccessor
    {
        public NativeList<Entity> Entities;
    }
}