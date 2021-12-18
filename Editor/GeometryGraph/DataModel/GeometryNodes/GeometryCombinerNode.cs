using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public abstract class GeometryCombinerNode : GeometryNodeModel, IGeometryNode
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = AddExecutionOutput(nameof(GeometryOut));
        }

        public abstract void Resolve(EditorGeometryGraphResolverContext context, GeometryStackData parent);
    }
}