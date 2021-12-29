using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance);
    }
}