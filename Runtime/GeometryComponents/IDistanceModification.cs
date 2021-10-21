using Code.SIMDMath;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer);
    }
}