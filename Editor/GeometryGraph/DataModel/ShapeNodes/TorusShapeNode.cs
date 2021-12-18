using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    [Serializable]
    public class TorusShapeNode : ShapeNode<CShapeTorus>
    {
        public override string Title
        {
            get => "Torus";
            set { }
        }

        public IPortModel RadiusIn { get; set; }
        public IPortModel Thickness { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue: 8);
            Thickness = this.AddDataInputPort<float>(nameof(Thickness), defaultValue: 3);
        }

        protected override ShapeType GetShapeType()
        {
            return ShapeType.Torus;
        }

        public override List<GeometryGraphProperty> GetProperties(EditorGeometryGraphResolverContext context)
        {
            return new() {RadiusIn.ResolvePropertyInput(context, GeometryPropertyType.Float), Thickness.ResolvePropertyInput(context, GeometryPropertyType.Float)};
        }
    }
}