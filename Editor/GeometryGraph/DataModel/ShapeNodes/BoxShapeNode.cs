using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class BoxShapeNode : ShapeNode<TorusShapeResolver>
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

        protected override ShapeProxy GetShape(EditorGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            throw new System.NotImplementedException();
        }

        public override List<GeometryGraphProperty> GetProperties(EditorGeometryGraphResolverContext context)
        {
            return new() {ExtendsIn.ResolvePropertyInput(context, GeometryPropertyType.Float3)};
        }
    }
}