using Code.SIMDMath;
using JetBrains.Annotations;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface IGeometryShape
    {
        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS);

        ShapeType ShapeType { get; }
    }
}