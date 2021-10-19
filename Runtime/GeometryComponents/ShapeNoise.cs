﻿using System.Runtime.InteropServices;
using Code.SIMDMath;
using Rendering;
using Unity.Collections;

using Unity.Mathematics;

namespace GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeNoise : ITerrainModifierShape
    {
        [FieldOffset(0)] public FloatValue strength;
        [FieldOffset(4)] public FloatValue valueOffset;
        [FieldOffset(8)] public Float3Value offset;
        [FieldOffset(12)] public Float3Value scale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            var offsetValue = offset.Resolve(valueBuffer);

            var positionOS = scale.Resolve(valueBuffer) * (positionWS - offsetValue);
            return ((cnoise4(positionOS)) + valueOffset.Resolve(valueBuffer)) * strength.Resolve(valueBuffer);
        }

        public ShapeType Type => ShapeType.Noise;

        private PackedFloat cnoise4(PackedFloat3 input)
        {
            PackedFloat result = default;
            result.PackedValues[0] = NoiseSlice(input, 0);
            result.PackedValues[1] = NoiseSlice(input, 1);
            result.PackedValues[2] = NoiseSlice(input, 2);
            result.PackedValues[3] = NoiseSlice(input, 3);
            return result;
        }

        private static float NoiseSlice(PackedFloat3 input, int slice)
        {
            return noise.cnoise(-new float3(input.x.PackedValues[slice], input.y.PackedValues[slice], input.z.PackedValues[slice]));
        }
    }
}