using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
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