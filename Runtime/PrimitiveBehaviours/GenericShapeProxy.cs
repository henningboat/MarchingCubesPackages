using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class GenericShapeProxy : ShapeProxy
    {
        public GenericShapeProxy(ShapeType shapeType, GeometryGraphProperty transformation) : base(transformation)
        {
            ShapeType = shapeType;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>();
        }

        public override ShapeType ShapeType { get; }
    }
}