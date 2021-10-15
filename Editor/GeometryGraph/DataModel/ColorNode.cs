using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    public class ColorNode : NodeModel, IGeometryNode
    {
        private IPortModel _geometryIn;
        private IPortModel _colorIn;
        private IPortModel _geometryOut;

        protected override void OnDefineNode()
        {
            _geometryIn = this.AddExecutionInputPort("", nameof(_geometryIn));
            _geometryOut = this.AddExecutionOutputPort("", nameof(_geometryOut));
            _colorIn = this.AddDataInputPort<Color>("", nameof(_colorIn));
        }

        public void Resolve(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            var colorFloat3Property = _colorIn.ResolvePropertyInput(context, GeometryPropertyType.Float3);
            var color32Property = context.GetOrCreateProperty(Guid,
                new GeometryGraphMathOperatorProperty(context, GeometryPropertyType.Color32, MathOperatorType.Float3ToColor32, colorFloat3Property, context.ZeroFloatProperty,
                    "Convert Float3 to Color"));
            stackData.Color = color32Property;
            _geometryIn.ResolveGeometryInput(context, stackData);
        }
    }
}