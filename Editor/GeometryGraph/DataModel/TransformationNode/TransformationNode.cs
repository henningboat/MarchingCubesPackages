using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public abstract class TransformationNode : GeometryNodeModel, IGeometryNode
    {
        private IPortModel _geometryInput;
        private IPortModel _geometryOutput;

        protected override void OnDefineNode()
        {
            _geometryInput = AddExecutionInput(nameof(_geometryInput));
            _geometryOutput = AddExecutionOutput(nameof(_geometryInput));
        }

        protected abstract GeometryGraphProperty GetTransformationProperty(GeometryInstructionListBuilder context,
            GeometryGraphProperty parent);

        public void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData)
        {
            stackData.Transformation = GetTransformationProperty(context, stackData.Transformation);
            _geometryInput.ResolveGeometryInput(context, stackData);
        }
    }
}