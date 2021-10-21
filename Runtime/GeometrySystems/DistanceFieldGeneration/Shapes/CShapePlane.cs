using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapePlane :  ITerrainModifierShape
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            return positionWS.y;
        }

        public ShapeType Type => ShapeType.Plane;

    }
}