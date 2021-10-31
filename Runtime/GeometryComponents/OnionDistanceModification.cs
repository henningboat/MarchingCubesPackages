using Code.SIMDMath;

namespace henningboat.CubeMarching.GeometryComponents
{
    public struct OnionDistanceModification: IDistanceModification
    {
        public float Thickness;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return SimdMath.abs(currentDistance) - Thickness;
        }
    }
}