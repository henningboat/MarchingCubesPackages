using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Sequential,Size = 16)]
    public struct CGenericDistanceModification: IDistanceModification
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