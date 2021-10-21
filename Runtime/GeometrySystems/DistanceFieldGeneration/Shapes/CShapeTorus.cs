using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeTorus : ITerrainModifierShape
    {
        [FieldOffset(0)] public FloatValue radius;
        [FieldOffset(4)] public FloatValue thickness;

        //SDF code from
        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        private PackedFloat sdTorus(PackedFloat3 p, PackedFloat radius, PackedFloat thickness)
        {
            var q = new PackedFloat2(SimdMath.length(new PackedFloat2(p.x, p.z)) - radius, p.y);
            return SimdMath.length(q) - thickness;
        }

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            return sdTorus(positionOS, radius.Resolve(valueBuffer), thickness.Resolve(valueBuffer));
        }
        
        public ShapeType Type => ShapeType.Torus;
    }
}