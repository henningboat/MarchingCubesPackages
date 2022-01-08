using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel
{
    public class ColorNode : GeometryNodeModel, IGeometryNode
    {
        private IPortModel _geometryIn;
        private IPortModel _colorIn;
        private IPortModel _geometryOut;

        protected override void OnDefineNode()
        {
            _geometryIn = AddExecutionInput(nameof(_geometryIn));
            _geometryOut = AddExecutionOutput(nameof(_geometryOut));
            _colorIn = this.AddDataInputPort<Color>("", nameof(_colorIn));
        }

        public void Resolve(GeometryInstructionListBuilder context)
        {
            GeometryGraphProperty colorFloat3Property = _colorIn.ResolvePropertyInput(context, GeometryPropertyType.Float3);
           context.AddMathInstruction(MathOperatorType.Float3ToColor32,GeometryPropertyType.Color32,
                 colorFloat3Property, context.ZeroFloatProperty,out GeometryGraphProperty colorProperty);

            context.PushColor(colorProperty);
            
            _geometryIn.ResolveGeometryInput(context);
            
            context.PopColor();
        }
    }
}