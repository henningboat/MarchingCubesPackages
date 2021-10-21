using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using DistanceFieldGeneration.Shapes;
using GeometryComponents;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class BoxShapeNode : ShapeNode<CShapeTorus>
    {
        public override string Title
        {
            get => "Torus";
            set { }
        }

        public IPortModel ExtendsIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            ExtendsIn = this.AddDataInputPort<Vector3>(nameof(ExtendsIn), defaultValue: Vector3.one * 4);
        }

        protected override ShapeType GetShapeType()
        {
            return ShapeType.Box;
        }

        public override List<GeometryGraphProperty> GetProperties(GeometryGraphResolverContext context)
        {
            return new() {ExtendsIn.ResolvePropertyInput(context, GeometryPropertyType.Float3)};
        }
    }
}