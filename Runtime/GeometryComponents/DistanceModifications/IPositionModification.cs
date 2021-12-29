using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    public interface IPositionModification
    {
        public TerrainTransformationType TerrainTransformationType { get; }
        public PackedFloat3 TransformPosition(PackedFloat3 positionWS);
    }
}