using Code.SIMDMath;
using JetBrains.Annotations;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface IGeometryShapeResolver
    {
        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS);
    }
}