using System.Runtime.InteropServices;
using Code.SIMDMath;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct PlaneShape : IGeometryShape
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            return positionWS.y;
        }

        public ShapeType ShapeType => ShapeType.Plane;
    }
}