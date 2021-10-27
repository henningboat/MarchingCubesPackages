using Code.SIMDMath;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    public struct InversionDistanceModification:IDistanceModification
    {
        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer)
        {
            return -currentDistance;
        }
    }
}