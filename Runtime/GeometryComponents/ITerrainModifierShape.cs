using Code.SIMDMath;
using JetBrains.Annotations;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface ITerrainModifierShape
    {
        ShapeType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer);
    }
}