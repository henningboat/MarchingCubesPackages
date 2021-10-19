using Code.SIMDMath;
using JetBrains.Annotations;
using Rendering;
using Unity.Collections;

namespace GeometryComponents
{
    public interface ITerrainModifierShape
    {
        ShapeType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer);
    }
}