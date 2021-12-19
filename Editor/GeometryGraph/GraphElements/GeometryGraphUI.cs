
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = System.Diagnostics.Debug;

namespace Code.CubeMarching.GeometryGraph.Editor.GraphElements
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
          
            geometryGraphViewWindow.SetLivePreviewNode(NodeModel, IsSelected());
                
        }
    }
}
