using Code.SIMDMath;

namespace henningboat.CubeMarching.GeometryComponents
{
    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance);
    }
}