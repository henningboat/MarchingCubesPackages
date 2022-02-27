using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using JetBrains.Annotations;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    public interface IGeometryShape
    {
        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, AssetDataStorage assetData);

        ShapeType Type { get; }
    }
}