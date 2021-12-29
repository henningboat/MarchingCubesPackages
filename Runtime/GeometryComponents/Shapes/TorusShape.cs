using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct TorusShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8.0f)] public float radius;
        [FieldOffset(4)] [DefaultValue(2.0f)] public float thickness;

        //SDF code from
        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        private PackedFloat sdTorus(PackedFloat3 p, PackedFloat radius, PackedFloat thickness)
        {
            var q = new PackedFloat2(SimdMath.length(new PackedFloat2(p.x, p.z)) - radius, p.y);
            return SimdMath.length(q) - thickness;
        }

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            return sdTorus(positionOS, radius, thickness);
        }

        public ShapeType ShapeType => ShapeType.Torus;
    }
}