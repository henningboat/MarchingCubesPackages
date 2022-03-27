using System;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace Editor.Debugging
{
    [Overlay(typeof(SceneView), "Geometry Field Overlays")]
    internal class GeometryFieldOverlay : Overlay
    {
        private Action<DebugInfo> _onDebuggingInfo;
        private VisualElement _root;

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement();

            var  mLabel = new Label();
            _root.Add(mLabel);
            
            
            _onDebuggingInfo = debugInfo =>
            {
                mLabel.text = "Chunks with Data " + debugInfo.chunksWithData;
            };
            GeometryFieldManager.OnDebuggingInfo += _onDebuggingInfo;

            return _root;
        }

        public override void OnWillBeDestroyed()
        {
            GeometryFieldManager.OnDebuggingInfo -= _onDebuggingInfo;
        }
    }
}