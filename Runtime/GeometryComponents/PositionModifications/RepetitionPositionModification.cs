using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RepetitionPositionModification : IPositionModification
    {
        [FieldOffset(0)] public float3 Period;

        public TransformationType Type => TransformationType.Repetition;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            positionWS += 1000 * Period;
            positionWS = SimdMath.mod(positionWS + 0.5f * Period, Period) - 0.5f * Period;
            return positionWS;
        }
    }
}