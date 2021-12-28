using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
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