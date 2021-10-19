using Code.SIMDMath;
using Unity.Collections;

namespace GeometryComponents
{
    public interface ITerrainTransformation
    {
        public TerrainTransformationType TerrainTransformationType { get; }
        public PackedFloat3 TransformPosition(PackedFloat3 positionWS, NativeArray<float> valueBuffer);
    }
}