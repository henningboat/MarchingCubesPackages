using Code.SIMDMath;
using Unity.Collections;

namespace GeometryComponents
{
    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer);
    }
}