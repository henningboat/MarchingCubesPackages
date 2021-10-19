using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections.LowLevel.Unsafe;

namespace TerrainChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedDistanceFieldData
    {
        public PackedFloat SurfaceDistance;
        public PackedTerrainMaterial TerrainMaterial;

        public PackedDistanceFieldData(PackedFloat surfaceDistance, PackedTerrainMaterial packedTerrainMaterial = default) : this()
        {
            SurfaceDistance = surfaceDistance;
            TerrainMaterial = packedTerrainMaterial;
        }

        public const int UnpackedCapacity = 4;

        public DistanceFieldData this[int i]
        {
            get
            {
                unsafe
                {

                    throw new NotImplementedException();
                    
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (i < 0 || i >= UnpackedCapacity)
                    {
                        throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                    }
#endif
                    var pointer = UnsafeUtility.AddressOf(ref this);
                    var arrayEntry = UnsafeUtility.ReadArrayElement<DistanceFieldData>(pointer, i);
                    return arrayEntry;
                }
            }
            set
            {
                unsafe
                {
                    throw new NotImplementedException();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (i < 0 || i >= UnpackedCapacity)
                    {
                        throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                    }
#endif
                    var pointer = UnsafeUtility.AddressOf(ref this);
                    var valueToWrite = value;
                    UnsafeUtility.WriteArrayElement(pointer, i, valueToWrite);
                }
            }
        }
    }
}