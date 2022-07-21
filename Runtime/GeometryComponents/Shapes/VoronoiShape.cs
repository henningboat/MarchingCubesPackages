using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct VoronoiShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(5f)] public float valueOffset;

        [FieldOffset(4)] [DefaultValue(30, 30, 30)]
        public float3 scale;

        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            for (var i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                var surfaceDistance = Voronoi(positionOS * (1f / (scale))) * scale.x + new PackedFloat(valueOffset);
                ;
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
        }

        public ShapeType Type => ShapeType.Voronoi;


        public static PackedFloat3 Hash(PackedFloat3 p3)
        {
            p3 = frac(p3 * new PackedFloat3(0.1031f, 0.1030f, (PackedFloat)0.0973f));
            p3 += dot(p3, new PackedFloat3(p3.y,p3.x,p3.z)+33.33f);
            return frac((new PackedFloat3(p3.x, p3.y, p3.y) + new PackedFloat3(p3.y, p3.x, p3.x)) *
                        new PackedFloat3(p3.z, p3.y, p3.x));
        }
        
        
        //from https://www.shadertoy.com/view/ldl3Dl
        public static PackedFloat Voronoi(PackedFloat3 x)
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
                var b = new PackedFloat3(new PackedFloat((float) i), new PackedFloat((float)j), new PackedFloat((float)k));
                var r = b - f + Hash(p + b);
                //var d = dot(r, r);
                var d = length(r);

                var newDistanceIsSmallest = d < res;

                res = select(res, d, newDistanceIsSmallest);
                res2 = select(min(res2, d), res2, newDistanceIsSmallest);
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