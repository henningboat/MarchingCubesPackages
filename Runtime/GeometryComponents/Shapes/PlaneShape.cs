using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct PlaneShape : IGeometryShape
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, AssetDataStorage assetData)
        {
            return positionWS.y;
        }

        public ShapeType Type => ShapeType.Plane;
    }
}