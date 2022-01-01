using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance);
        DistanceModificationType Type { get; }
    }
}