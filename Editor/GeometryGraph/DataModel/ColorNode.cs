using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
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

        public void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData)
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