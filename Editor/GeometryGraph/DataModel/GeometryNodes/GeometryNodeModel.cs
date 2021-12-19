using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public abstract class GeometryNodeModel : NodeModel
    {
        protected IPortModel AddExecutionInput(string portID)
        {
            return this.AddExecutionInputPort("", portID, PortOrientation.Vertical);
        }

        protected IPortModel AddExecutionOutput(string portID)
        {
            return this.AddExecutionOutputPort("", portID, PortOrientation.Vertical);
        }
    }
}