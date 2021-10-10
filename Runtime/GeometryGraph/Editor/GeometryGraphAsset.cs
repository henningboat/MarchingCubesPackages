using System;
using System.Linq;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    [Serializable]
    public class GeometryGraphAsset : GraphAssetModel
    {
        protected override Type GraphModelType => typeof(GeometryGraphModel);

        [MenuItem("Assets/Create/Geometry Graph")]
        public static void CreateGraph(MenuCommand menuCommand)
        {
            const string path = "Assets";
            var template = new GraphTemplate<GeometryGraphStencil>(GeometryGraphStencil.GraphName);
            CommandDispatcher commandDispatcher = null;
            if (EditorWindow.HasOpenInstances<SimpleGraphViewWindow>())
            {
                var window = EditorWindow.GetWindow<SimpleGraphViewWindow>();
                if (window != null)
                {
                    commandDispatcher = window.CommandDispatcher;
                }
            }

            GraphAssetCreationHelpers<GeometryGraphAsset>.CreateInProjectWindow(template, commandDispatcher, path);
        }

        [OnOpenAsset(1)]
        public static bool OpenGraphAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is GeometryGraphAsset graphAssetModel)
            {
                var window = GraphViewEditorWindow.FindOrCreateGraphWindow<SimpleGraphViewWindow>();
                window.SetCurrentSelection(graphAssetModel, GraphViewEditorWindow.OpenMode.OpenAndFocus);
                return window != null;
            }

            return false;
        }

        public GeometryGraphResolverContext ResolveGraph()
        {
            var context = new GeometryGraphResolverContext();

            var resultNode = GraphModel.NodeModels.FirstOrDefault(model => model is GraphResult) as GraphResult;

            var rootNode = resultNode.DataIn.GetConnectedPorts().FirstOrDefault().NodeModel as IGeometryNode;

            context.BeginWriteCombiner(new CombinerInstruction(CombinerOperation.Min,context.ZeroFloatProperty,context.CurrentCombinerDepth));
            
            rootNode.Resolve(context, context.OriginTransformation);

            context.FinishWritingCombiner();
            
            context.BuildBuffers();

            return context;
        }
    }
}