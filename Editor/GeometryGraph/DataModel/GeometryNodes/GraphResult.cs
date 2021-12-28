using System;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    [Serializable]
    public class GraphResult : GeometryNodeModel
    {
        public override string Title
        {
            get => "Result";
            set { }
        }

        public IPortModel DataIn { get; private set; }

        protected override void OnDefineNode()
        {
            DataIn = AddExecutionInput(nameof(DataIn));
        }
    }
}