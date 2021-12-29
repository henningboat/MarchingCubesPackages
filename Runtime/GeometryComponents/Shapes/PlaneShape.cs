using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
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