using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class GenericShapeProxy : ShapeProxy
    {
        private readonly GeometryGraphProperty[] _properties;

        public GenericShapeProxy(ShapeType shapeType, GeometryGraphProperty transformation, params GeometryGraphProperty[] properties) : base(transformation)
        {
            ShapeType = shapeType;
            _properties = properties;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return _properties.ToList();
        }

        public override ShapeType ShapeType { get; }
    }
}