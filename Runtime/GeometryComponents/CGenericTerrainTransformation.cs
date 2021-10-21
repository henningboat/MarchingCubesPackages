using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericTerrainTransformation
    {
        public int16 Data;
        public TerrainTransformationType TerrainTransformationType;

        public PackedFloat3 TransformPosition(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (TerrainTransformationType)
                {
                    //todo reimplement
                    // case TerrainTransformationType.Mirror:
                    //     return ((CTerrainTransformationMirror*) ptr)->TransformPosition(positionOS);
                    //     break;
                    case TerrainTransformationType.Repetition:
                        return ((CTerrainTransformationRepetition*) ptr)->TransformPosition(positionOS, valueBuffer);
                    case TerrainTransformationType.Transform:
                        return ((CGeometryTransformation*) ptr)->TransformPosition(positionOS, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // public uint CalculateHash()
        // {
        //     unsafe
        //     {
        //         var ptr = UnsafeUtility.AddressOf(ref Data);
        //         switch (TerrainTransformationType)
        //         {
        //             //todo reimplement
        //             // case TerrainTransformationType.Mirror:
        //             //     return ((CTerrainTransformationMirror*) ptr)->CalculateHash();
        //             case TerrainTransformationType.Transform:
        //                 return ((CGeometryTransformation*) ptr)->CalculateHash();
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        // }
    }
}