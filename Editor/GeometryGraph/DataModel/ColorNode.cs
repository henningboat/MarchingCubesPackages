using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
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
            throw new NotImplementedException();
            // var colorFloat3Property = _colorIn.ResolvePropertyInput(context, GeometryPropertyType.Float3);
            // var color32Property = context.CreateMathOperation(Guid,
            //     new GeometryGraphMathOperatorProperty(GeometryPropertyType.Color32, MathOperatorType.Float3ToColor32, colorFloat3Property, context.ZeroFloatProperty,
            //         "Convert Float3 to Color"));
            // stackData.Color = color32Property;
            // _geometryIn.ResolveGeometryInput(context, stackData);
        }
    }
}