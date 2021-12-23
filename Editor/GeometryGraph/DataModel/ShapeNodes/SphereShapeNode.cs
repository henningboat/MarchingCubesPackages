using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class SphereShapeNode : ShapeNode<SphereShapeResolver>
    {
        public override string Title
        {
            get => "Sphere";
            set { }
        }

        public IPortModel RadiusIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue: 8);
        }


        protected override ShapeProxy GetShape(EditorGeometryGraphResolverContext context,GeometryStackData stackData)
        {
            return new SphereShapeProxy(RadiusIn.ResolvePropertyInput(context, GeometryPropertyType.Float),
                stackData.Transformation);
        }

        public override List<GeometryGraphProperty> GetProperties(EditorGeometryGraphResolverContext context)
        {
            return new() {RadiusIn.ResolvePropertyInput(context, GeometryPropertyType.Float)};
        }
    }
}