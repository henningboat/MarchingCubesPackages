using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.Utils.Containers;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class SphereShapeProxy : ShapeProxy
    {
        private GeometryGraphProperty _radius;

        public SphereShapeProxy(GeometryGraphProperty radius, GeometryGraphProperty transformation) : base(transformation)
        {
            _radius = radius;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>()
            {
                _radius
            };
        }

        public override int GeometryInstructionSubType { get; }
    }
}