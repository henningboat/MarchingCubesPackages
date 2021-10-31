using Code.SIMDMath;
using JetBrains.Annotations;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface ITerrainModifierShape
    {
        ShapeType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS);
    }
}