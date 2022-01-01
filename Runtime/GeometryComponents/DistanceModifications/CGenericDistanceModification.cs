using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using SIMDMath;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct CGenericDistanceModification
    {
        public float32 Data;
        public DistanceModificationType Type;

        public PackedFloat GetSurfaceDistance(PackedFloat surfaceDistance)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (Type)
                {
                    case DistanceModificationType.Onion:
                        return ((OnionDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance);
                    case DistanceModificationType.Inversion:
                        return ((InversionDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance);
                    case DistanceModificationType.Inflation:
                        return ((InflationDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}