using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using SIMDMath;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericTerrainTransformation
    {
        public float32 Data;
        public TransformationType TransformationType;

        public PackedFloat3 TransformPosition(PackedFloat3 positionOS)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (TransformationType)
                {
                    //todo reimplement
                    // case TerrainTransformationType.Mirror:
                    //     return ((CTerrainTransformationMirror*) ptr)->TransformPosition(positionOS);
                    //     break;
                    case TransformationType.Repetition:
                        return UnsafeCastHelper.Cast<float32, RepetitionPositionModification>(ref Data)
                            .TransformPosition(positionOS);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public static class UnsafeCastHelper
    {
        public static unsafe TOut Cast<TIn, TOut>(ref TIn data) where TIn : unmanaged where TOut : unmanaged
        {
            var ptr = UnsafeUtility.AddressOf(ref data);
            return *(TOut*) ptr;
        }
    }
}