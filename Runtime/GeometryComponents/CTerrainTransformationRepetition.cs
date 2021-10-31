using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometryComponents
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct CTerrainTransformationRepetition : ITerrainTransformation
    {
        [FieldOffset(0)] public float3 Period;

        public TerrainTransformationType TerrainTransformationType => TerrainTransformationType.Repetition;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            positionWS += 1000 * Period;
           positionWS= SimdMath.mod(positionWS + 0.5f * Period, Period) - 0.5f * Period;
           return positionWS;
        }
    }
}