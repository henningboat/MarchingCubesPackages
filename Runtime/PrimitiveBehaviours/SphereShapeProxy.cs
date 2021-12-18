using System.Collections.Generic;
using henningboat.CubeMarching.Utils.Containers;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class SphereShapeProxy : ShapeProxy
    {
        private GeometryGraphValue _radius;

        public SphereShapeProxy(GeometryGraphValue radius)
        {
            _radius = radius;
        }

        protected override List<GeometryGraphValue> GetProperties()
        {
            return new List<GeometryGraphValue>()
            {
                _radius
            };
        }

        public override int GeometryInstructionSubType { get; }
    }
}