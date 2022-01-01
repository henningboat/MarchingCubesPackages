using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RepetitionPositionModification : IPositionModification
    {
        [FieldOffset(0)] [DefaultValue(16, 16, 16)]
        public float3 Period;

        public PositionModificationType Type => PositionModificationType.Repetition;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            positionWS += 1000 * Period;
            positionWS = SimdMath.mod(positionWS + 0.5f * Period, Period) - 0.5f * Period;
            return positionWS;
        }
    }
}