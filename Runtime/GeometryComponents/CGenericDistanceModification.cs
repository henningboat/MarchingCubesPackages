using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace GeometryComponents
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
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}