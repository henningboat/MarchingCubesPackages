using System.Collections.Generic;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;

namespace Editor.GeometryGraph
{
    internal class GeometryGraphViewWindow : GraphViewEditorWindow
    {
        private IGeometryNode _currentlyPreviewedNode;

        public bool LivePreviewEnabled { get; private set; }

        protected override void OnEnable()
        {
            EditorToolName = "Geometry Graph";
            base.OnEnable();
        }

        [InitializeOnLoadMethod]
        private static void RegisterTool()
        {
            ShortcutHelper.RegisterDefaultShortcuts<GeometryGraphViewWindow>(GeometryGraphStencil.GraphName);
        }

        [MenuItem("Window/Geometry Graph Editor")]
        public static void ShowWindow()
        {
            GetWindow<GeometryGraphViewWindow>();
        }

        protected override GraphToolState CreateInitialState()
        {
            var prefs = Preferences.CreatePreferences(EditorToolName);
            return new GeometryGraphState(GUID, prefs);
        }

        protected override GraphView CreateGraphView()
        {
            return new GraphView(this, CommandDispatcher, EditorToolName);
        }

        protected override BlankPage CreateBlankPage()
        {
            var onboardingProviders = new List<OnboardingProvider>();
            onboardingProviders.Add(new GeometryGraphOnboardingProvider());

            return new BlankPage(CommandDispatcher, onboardingProviders);
        }

        protected override bool CanHandleAssetType(IGraphAssetModel asset)
        {
            return asset is GeometryGraphAsset;
        }

        protected override MainToolbar CreateMainToolbar()
        {
            return new GeometryGraphMainToolbar(CommandDispatcher, GraphView);
        }

        public void OnLivePreviewToggle(ChangeEvent<bool> evt)
        {
            LivePreviewEnabled = evt.newValue;
            if (evt.newValue == false)
            {
                GeometryGraphInstance.ClearDebugOverwrite();
            }
        }

        public void SetLivePreviewNode(INodeModel node, bool isSelected)
        {
            if (LivePreviewEnabled)
            {
                if (isSelected)
                {
                    SetupPreviewNode(node);
                }
                else
                {
                    if (node == _currentlyPreviewedNode) GeometryGraphInstance.ClearDebugOverwrite();
                }
            }
            else
            {
                GeometryGraphInstance.ClearDebugOverwrite();
            }
        }

        private void SetupPreviewNode(INodeModel node)
        {
            _currentlyPreviewedNode = node as IGeometryNode;
            if (_currentlyPreviewedNode == null) return;

            var data = CreateInstance<GeometryGraphRuntimeAsset>();
            GeometryGraphProcessor.ResolveGraphAndWriteToRuntimeData(_currentlyPreviewedNode, data,
                GraphView.GraphModel);

            GeometryGraphInstance.SetDebugOverwrite(GraphView.GraphModel.AssetModel as GeometryGraphAsset, data);
        }
    }
}