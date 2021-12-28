using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathNode : GeometryNodeModel
    {
        public abstract void ResetConnections();
    }
}