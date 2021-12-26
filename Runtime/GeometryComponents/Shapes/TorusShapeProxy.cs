using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [ShapeProxy(ShapeType.Torus)]
    public class TorusShapeProxy : ShapeProxy
    {
        [PropertyType(GeometryPropertyType.Float)]
        private GeometryGraphProperty _radius;

        [PropertyType(GeometryPropertyType.Float)]
        private GeometryGraphProperty _thickness;

        public TorusShapeProxy(GeometryGraphProperty radius, GeometryGraphProperty thickness,
            GeometryGraphProperty transformation) : base(transformation)
        {
            _radius = radius;
            _thickness = thickness;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>()
            {
                _radius,
                _thickness
            };
        }

        public override ShapeType ShapeType => ShapeType.Plane;
    }


    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct TorusShapeResolver : IGeometryShapeResolver
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