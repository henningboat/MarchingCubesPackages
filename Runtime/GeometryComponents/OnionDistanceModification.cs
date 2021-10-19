using Code.SIMDMath;
using Unity.Collections;

namespace GeometryComponents
{
    public struct OnionDistanceModification: IDistanceModification
    {
        public FloatValue Thickness;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer)
        {
            return SimdMath.abs(currentDistance) - Thickness.Resolve(valueBuffer);
        }
    }
}