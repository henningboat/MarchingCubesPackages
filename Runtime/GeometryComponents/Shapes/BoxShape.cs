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
    public struct BoxShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8, 8, 8)]
        public float3 Extends;

        public PackedFloat GetSurfaceDistance(in PackedFloat3 positionOS, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            var extends = Extends;
            var q = abs(positionOS) - extends;
            return length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
        }

        public ShapeType Type => ShapeType.Box;
    }
}