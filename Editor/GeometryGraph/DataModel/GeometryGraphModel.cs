using System;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    [Serializable]
    public class GeometryGraphModel : GraphModel
    {
        public GeometryGraphModel()
        {
            StencilType = null;
        }
        public override Type DefaultStencilType => typeof(GeometryGraphStencil);

        public void OnLivePreviewToggle(ChangeEvent<bool> evt)
        {
            Debug.Log("preview");
        }
    }
}
