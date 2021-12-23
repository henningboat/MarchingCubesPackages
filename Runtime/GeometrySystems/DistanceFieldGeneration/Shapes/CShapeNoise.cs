using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeNoise : IGeometryShapeResolver
    {
        [FieldOffset(0)] public float strength;
        [FieldOffset(4)] public float valueOffset;
        [FieldOffset(20)] public float3 offset;
        [FieldOffset(32)] public float3 scale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            var offsetValue = offset;

            var positionOS = scale * (positionWS - offsetValue);
            return ((cnoise4(positionOS)) + valueOffset) * strength;
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
    public struct CShapeVoronoi : IGeometryShapeResolver
    {
        [FieldOffset(4)] public float valueOffset;
        [FieldOffset(20)] public float3 scale;
        public ShapeType Type => ShapeType.Noise;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            positionOS *= scale;
            return (Voronoi(positionOS) - new PackedFloat(valueOffset)) / scale.x;
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
            PackedFloat res2 = new PackedFloat(100f);
            
            
            for (int k = -1; k <= 1; k++)
            for (int j = -1; j <= 1; j++)
            for (int i = -1; i <= 1; i++)
            {
                PackedFloat3 b = new PackedFloat3(new PackedFloat(i), new PackedFloat(j), new PackedFloat(k));
                PackedFloat3 r =  b - f + Hash(p + b);
                PackedFloat d = SimdMath.dot(r, r);

                var newDistanceIsSmallest = d.PackedValues < res.PackedValues;
                
                res.PackedValues = math.@select(res.PackedValues, d.PackedValues, newDistanceIsSmallest);
                res2.PackedValues = math.@select(SimdMath.min(res2, d).PackedValues, res2.PackedValues, newDistanceIsSmallest);

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

            return -(res2 - res);
        }

    }
}