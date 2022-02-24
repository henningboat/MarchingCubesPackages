using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct VoronoiShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(0.3f)] public float valueOffset;

        [FieldOffset(4)] [DefaultValue(0.1f, 0.1f, 0.1f)]
        public float3 scale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            positionOS *= 1/scale;
            return Voronoi(positionOS)*scale.x + new PackedFloat(valueOffset);
        }

        public ShapeType Type => ShapeType.Voronoi;


        public PackedFloat3 Hash(PackedFloat3 x)
        {
            x = new PackedFloat3(
                dot(x, new PackedFloat3(new PackedFloat(127.1f), new PackedFloat(311.7f), new PackedFloat(74.7f))),
                dot(x, new PackedFloat3(new PackedFloat(269.5f), new PackedFloat(183.3f), new PackedFloat(246.1f))),
                dot(x, new PackedFloat3(new PackedFloat(113.5f), new PackedFloat(271.9f), new PackedFloat(124.6f))));

            return frac(sin(x) * 43758.5453123f);
        }

        //from https://www.shadertoy.com/view/ldl3Dl
        public PackedFloat Voronoi(PackedFloat3 x)
        {
            var p = floor(x);
            var f = frac(x);

            PackedFloat id = 0.0f;

            var res = new PackedFloat(100f);
            var res2 = new PackedFloat(100f);


            for (var k = -1; k <= 1; k++)
            for (var j = -1; j <= 1; j++)
            for (var i = -1; i <= 1; i++)
            {
                var b = new PackedFloat3(new PackedFloat(i), new PackedFloat(j), new PackedFloat(k));
                var r = b - f + Hash(p + b);
                //var d = dot(r, r);
                var d = length(r);

                var newDistanceIsSmallest = d.PackedValues < res.PackedValues;

                res.PackedValues = math.select(res.PackedValues, d.PackedValues, newDistanceIsSmallest);
                res2.PackedValues = math.select(min(res2, d).PackedValues, res2.PackedValues,
                    newDistanceIsSmallest);
            }

            return -(res2 - res);
        }
    }

    //original noise implementation
    // [StructLayout(LayoutKind.Explicit1, Size = 4 * 16)]
    // public struct CShapeNoise : IGeometryShapeResolver
    // {
    //     [FieldOffset(0)] public float strength;
    //     [FieldOffset(4)] public float valueOffset;
    //     [FieldOffset(20)] public float3 offset;
    //     [FieldOffset(32)] public float3 scale;
    //
    //     public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
    //     {
    //         var offsetValue = offset;
    //
    //         var positionOS = scale * (positionWS - offsetValue);
    //         return ((cnoise4(positionOS)) + valueOffset) * strength;
    //     }
    //
    //     public ShapeType Type => ShapeType.Noise;
    //
    //     private PackedFloat cnoise4(PackedFloat3 input)
    //     {
    //         PackedFloat result = default;
    //         result.PackedValues[0] = NoiseSlice(input, 0);
    //         result.PackedValues[1] = NoiseSlice(input, 1);
    //         result.PackedValues[2] = NoiseSlice(input, 2);
    //         result.PackedValues[3] = NoiseSlice(input, 3);
    //         return result;
    //     }
    //
    //     private static float NoiseSlice(PackedFloat3 input, int slice)
    //     {
    //         return noise.cnoise(-new float3(input.x.PackedValues[slice], input.y.PackedValues[slice],
    //             input.z.PackedValues[slice]));
    //     }
    // }
}