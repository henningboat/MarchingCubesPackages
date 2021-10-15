using Code.SIMDMath;
using JetBrains.Annotations;
using Rendering;
using Unity.Collections;
using Unity.Transforms;

namespace GeometryComponents
{
    public interface ITerrainModifierShape
    {
        ShapeType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer);

        [UsedImplicitly]
        TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer);

        uint CalculateHash();
    }
}