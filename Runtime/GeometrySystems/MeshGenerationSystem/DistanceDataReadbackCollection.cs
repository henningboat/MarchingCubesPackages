using System;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public struct DistanceDataReadbackCollection
    {
        private UnsafeList<PackedDistanceFieldData> _data0;
        private UnsafeList<PackedDistanceFieldData> _data1;
        private UnsafeList<PackedDistanceFieldData> _data2;
        private UnsafeList<PackedDistanceFieldData> _data3;

        unsafe public UnsafeList<PackedDistanceFieldData> this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (UnsafeList<PackedDistanceFieldData> * array = &_data0) { return array[index]; }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (UnsafeList<PackedDistanceFieldData>* array = &_data0) { array[index] = value; }
            }
        }
    }
}