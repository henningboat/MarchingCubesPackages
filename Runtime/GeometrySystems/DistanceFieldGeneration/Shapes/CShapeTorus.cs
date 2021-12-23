using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeTorus : IGeometryShapeResolver
    {
        [FieldOffset(0)] public float radius;
        [FieldOffset(4)] public float thickness;

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
        
        public ShapeType Type => ShapeType.Torus;
    }
}