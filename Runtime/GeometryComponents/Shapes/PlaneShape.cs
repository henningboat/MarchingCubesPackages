using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct PlaneShape : IGeometryShape
    {
        public PackedFloat GetSurfaceDistance(in PackedFloat3 positionOS, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            return positionOS.y;
        }

        public ShapeType Type => ShapeType.Plane;
    }
}