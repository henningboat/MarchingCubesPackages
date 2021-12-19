using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathNode : GeometryNodeModel
    {
        public abstract void ResetConnections();
    }
}