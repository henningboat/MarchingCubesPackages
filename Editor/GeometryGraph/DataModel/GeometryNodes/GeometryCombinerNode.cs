using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    public abstract class GeometryCombinerNode : GeometryNodeModel, IGeometryNode
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = AddExecutionOutput(nameof(GeometryOut));
        }

        public abstract void Resolve(GeometryInstructionListBuilder context, GeometryStackData parent);
    }
}