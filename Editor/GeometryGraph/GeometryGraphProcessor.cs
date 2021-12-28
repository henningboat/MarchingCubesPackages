using System;
using System.Linq;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    internal class GeometryGraphProcessor : IGraphProcessor
    {
        public static void ResolveGraphAndWriteToRuntimeData(IGeometryNode rootNode, GeometryGraphRuntimeAsset data,
            IGraphModel graphModel)
        {
            var result = Resolve(graphModel, rootNode);

            var contentHash = new Hash128();

            data.Initialize(result.GetGeometryGraphData());

            result.Dispose();
        }

        public GraphProcessingResult ProcessGraph(IGraphModel graphModel)
        {
            EditorApplication.QueuePlayerLoopUpdate();

            try
            {
                var resultNode = graphModel.NodeModels.FirstOrDefault(model => model is GraphResult) as GraphResult;

                var rootNode = resultNode.DataIn.GetConnectedPorts().FirstOrDefault().NodeModel as IGeometryNode;

                var data = GetRuntimeData(graphModel);

                ResolveGraphAndWriteToRuntimeData(rootNode, data, graphModel);

                EditorUtility.SetDirty(data);
                return new GraphProcessingResult() {Errors = { }};
            }
            catch (Exception e)
            {
                var graphProcessingResult = new GraphProcessingResult();
                graphProcessingResult.AddError(e.Message);
                return graphProcessingResult;
            }
        }

        private static GeometryGraphRuntimeAsset GetRuntimeData(IGraphModel graphModel)
        {
            var assetPath = graphModel.AssetModel.GetPath();
            var data = AssetDatabase.LoadAssetAtPath<GeometryGraphRuntimeAsset>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<GeometryGraphRuntimeAsset>();
                data.name = graphModel.Name;
                AssetDatabase.AddObjectToAsset(data, assetPath);
                AssetDatabase.ImportAsset(assetPath);
            }

            return data;
        }

        private static GeometryInstructionListBuilder Resolve(IGraphModel graphModel, IGeometryNode rootNode)
        {
            var context = new GeometryInstructionListBuilder();

            context.BeginWriteCombiner(new CombinerState(CombinerOperation.Min, context.ZeroFloatProperty));

            rootNode.Resolve(context,
                new GeometryStackData() {Color = default, Transformation = context.OriginTransformation});

            context.FinishWritingCombiner();

            return context;
        }
    }
}