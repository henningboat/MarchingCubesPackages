using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public class ResetPositionNode : GeometryNodeModel, IGeometryNode
    {
        private IPortModel _geometryIn;

        public IPortModel GeometryOut { get; set; }
        
        protected override void OnDefineNode()
        {
            _geometryIn = AddExecutionInput(nameof(_geometryIn));
            GeometryOut = AddExecutionOutput(nameof(GeometryOut));
            base.OnDefineNode();
        }


        public void Resolve(GeometryInstructionListBuilder context)
        {
            context.PushTransformation(context.ZeroTransformation, false);
          
            _geometryIn.ResolveGeometryInput(context);
            context.PopTransformation();
        }
    }
}