using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericDistanceModification: IDistanceModification
    {
        public int16 Data;
        public DistanceModificationType Type;
        
        public PackedFloat GetSurfaceDistance(PackedFloat surfaceDistance, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (Type)
                {
                    case DistanceModificationType.Onion:
                        return ((OnionDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance, valueBuffer);
                    case DistanceModificationType.Inversion:
                        return ((InversionDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance, valueBuffer);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}