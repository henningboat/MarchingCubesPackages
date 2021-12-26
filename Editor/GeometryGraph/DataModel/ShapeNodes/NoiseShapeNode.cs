using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class NoiseShapeNode : ShapeNode<VoronoiShapeResolver>
    {
        public override string Title
        {
            get => "Noise";
            set { }
        }

        public IPortModel StrengthIn { get; set; }
        public IPortModel ValueOffsetIn { get; set; }
        public IPortModel PositionOffsetIn { get; set; }
        public IPortModel ScaleIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            StrengthIn = this.AddDataInputPort<float>("Strength", nameof(StrengthIn), defaultValue: 1);
            ValueOffsetIn = this.AddDataInputPort<float>("ValueOffset", nameof(ValueOffsetIn), defaultValue: 0);
            PositionOffsetIn = this.AddDataInputPort<Vector3>("OffsetIn", nameof(PositionOffsetIn), defaultValue: Vector3.zero);
            ScaleIn = this.AddDataInputPort<Vector3>("Scale", nameof(ScaleIn), defaultValue: Vector3.one * 8);
        }

        protected override ShapeProxy GetShape(EditorGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            throw new System.NotImplementedException();
        }

        public override List<GeometryGraphProperty> GetProperties(EditorGeometryGraphResolverContext context)
        {
            return new()
            {
                StrengthIn.ResolvePropertyInput(context, GeometryPropertyType.Float),
                ValueOffsetIn.ResolvePropertyInput(context, GeometryPropertyType.Float),
                PositionOffsetIn.ResolvePropertyInput(context, GeometryPropertyType.Float3),
                ScaleIn.ResolvePropertyInput(context, GeometryPropertyType.Float3)
            };
        }
    }

}