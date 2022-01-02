using System;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    [Serializable]
    public class ResultNode : GeometryNodeModel,IGeometryNode
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

        public void Resolve(GeometryInstructionListBuilder context)
        {
            DataIn.ResolveGeometryInput(context);
        }
    }
}