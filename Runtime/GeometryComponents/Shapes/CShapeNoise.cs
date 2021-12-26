using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
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