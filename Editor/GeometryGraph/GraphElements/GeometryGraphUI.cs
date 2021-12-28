using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.GraphElements
{
    public class GeometryGraphUI : CollapsibleInOutNode
    {
        public static readonly string printResultPartName = "print-result";

        protected override void BuildPartList()
        {
            base.BuildPartList();

            //PartList.AppendPart(PrintResultPart.Create(printResultPartName, Model, this, ussClassName));
        }

        protected override void UpdateElementFromModel()
        {
            base.UpdateElementFromModel();
            var geometryGraphViewWindow = GraphView.Window as GeometryGraphViewWindow;

            if (geometryGraphViewWindow != null)
            {
                geometryGraphViewWindow.SetLivePreviewNode(NodeModel, IsSelected());
            }
        }
    }
}
