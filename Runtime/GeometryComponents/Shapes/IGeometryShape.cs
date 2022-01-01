using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using JetBrains.Annotations;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    public interface IGeometryShape
    {
        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS);

        ShapeType Type { get; }
    }
}