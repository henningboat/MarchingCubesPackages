﻿using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
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
            return noise.cnoise(-new float3(input.x.PackedValues[slice], input.y.PackedValues[slice],
                input.z.PackedValues[slice]));
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeVoronoi : ITerrainModifierShape
    {
        [FieldOffset(4)] public FloatValue valueOffset;
        [FieldOffset(12)] public Float3Value scale;
        public ShapeType Type => ShapeType.Noise;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            positionOS *= scale.Resolve(valueBuffer);
            return Voronoi(positionOS) - new PackedFloat(valueOffset.Resolve(valueBuffer));
        }


        public PackedFloat3 Hash(PackedFloat3 x)
        {
            x = new PackedFloat3(
                SimdMath.dot(x,
                    new PackedFloat3(new PackedFloat(127.1f), new PackedFloat(311.7f), new PackedFloat(74.7f))),
                SimdMath.dot(x,
                    new PackedFloat3(new PackedFloat(269.5f), new PackedFloat(183.3f), new PackedFloat(246.1f))),
                SimdMath.dot(x,
                    new PackedFloat3(new PackedFloat(113.5f), new PackedFloat(271.9f), new PackedFloat(124.6f))));

            return SimdMath.frac(SimdMath.sin(x) * 43758.5453123f);
        }

        //from https://www.shadertoy.com/view/ldl3Dl
        public PackedFloat Voronoi(PackedFloat3 x)
        {
            PackedFloat3 p = SimdMath.floor(x);
            PackedFloat3 f = SimdMath.frac(x);

            PackedFloat id = 0.0f;

            PackedFloat res = new PackedFloat(100f);
            for (int k = -1; k <= 1; k++)
            for (int j = -1; j <= 1; j++)
            for (int i = -1; i <= 1; i++)
            {
                PackedFloat3 b = new PackedFloat3(new PackedFloat(i), new PackedFloat(j), new PackedFloat(k));
                PackedFloat3 r =  b - f + Hash(p + b);
                PackedFloat d = SimdMath.dot(r, r);

                bool4 greater = d.PackedValues < res.PackedValues;

                for (int packedFloatIndex = 0; packedFloatIndex < 4; packedFloatIndex++)
                {
                    if (greater[packedFloatIndex])
                    {
                        res = d;
                    }
                }
              
                
                // if (d < res.x)
                // {
                //     id = dot(p + b, vec3(1.0f, 57.0f, 113.0f));
                //     res = vec2(d, res.x);
                // }
                // else if (d < res.y)
                // {
                //     res.y = d;
                // }
            }

            return SimdMath.sqrt(res);

            // return vec3(sqrt(res), abs(id));

        }

    }
}