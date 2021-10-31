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
        public float32 Data;
        public TerrainTransformationType TerrainTransformationType;

        public PackedFloat3 TransformPosition(PackedFloat3 positionOS)
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
                        return UnsafeCastHelper.Cast<float32,CTerrainTransformationRepetition>(ref Data).TransformPosition(positionOS);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public static class UnsafeCastHelper
    {
        public unsafe static TOut Cast<TIn,TOut>(ref TIn data) where TIn : unmanaged where TOut:unmanaged
        {
            var ptr = UnsafeUtility.AddressOf(ref data);
            return *(TOut*) ptr;
        }
    }
}