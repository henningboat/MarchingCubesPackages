using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public abstract class DistanceModificationNode : NodeModel, IGeometryNode
    {
        private IPortModel _geometryInput;
        private IPortModel _geometryOutput;

        protected override void OnDefineNode()
        {
            _geometryInput = this.AddExecutionInputPort("", nameof(_geometryInput));
            _geometryOutput = this.AddExecutionOutputPort("", nameof(_geometryInput));
        }


        public void Resolve(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            _geometryInput.ResolveGeometryInput(context, stackData);
            context.WriteDistanceModifier(GetDistanceModifierInstruction(context, stackData));
        }

        protected abstract DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext geometryGraphResolverContext, GeometryStackData stackData);
    }
}