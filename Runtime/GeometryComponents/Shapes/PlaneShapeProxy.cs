using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.PrimitiveBehaviours;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    [ShapeProxy(ShapeType.Plane)]
    public class PlaneShapeProxy : ShapeProxy
    {
        public PlaneShapeProxy(GeometryGraphProperty transformation) : base(transformation)
        {
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>();
        }

        public override ShapeType ShapeType => ShapeType.Plane;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct PlaneShapeResolver : IGeometryShapeResolver
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            return positionWS.y;
        }

        public ShapeType Type => ShapeType.Plane;
    }
}