using System.Runtime.InteropServices;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    [StructLayout(LayoutKind.Sequential,Size = 16)]
    public struct InversionDistanceModification : IDistanceModification
    {
        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return -currentDistance;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InflationDistanceModification : IDistanceModification
    {
        [FieldOffset(0)] private readonly float inflationAmount;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return currentDistance + inflationAmount;
        }
    }
}