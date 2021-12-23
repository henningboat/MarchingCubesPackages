using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapePlane :  IGeometryShapeResolver
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            return positionWS.y;
        }

        public ShapeType Type => ShapeType.Plane;

    }
}