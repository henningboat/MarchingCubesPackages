using Code.SIMDMath;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface ITerrainTransformation
    {
        public TerrainTransformationType TerrainTransformationType { get; }
        public PackedFloat3 TransformPosition(PackedFloat3 positionWS);
    }
}