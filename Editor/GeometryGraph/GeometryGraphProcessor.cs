using System;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    internal class GeometryGraphProcessor : IGraphProcessor
    {
        public GraphProcessingResult ProcessGraph(IGraphModel graphModel)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            
            try
            {
                var result = Resolve(graphModel);

                var contentHash = new Hash128();
                contentHash.Append(result.PropertyValueBuffer);
                contentHash.Append(result.MathInstructionBuffer);
                contentHash.Append(result.GeometryInstructionBuffer.ToArray());

                var data = GetRuntimeData(graphModel);
                data.InitializeData(result.PropertyValueBuffer, result.MathInstructionBuffer,
                    result.GeometryInstructionBuffer, contentHash,
                    new Float4X4Value {Index = result.OriginTransformation.Index});
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

        private static GeometryGraphRuntimeData GetRuntimeData(IGraphModel graphModel)
        {
            var assetPath = graphModel.AssetModel.GetPath();
            var data = AssetDatabase.LoadAssetAtPath<GeometryGraphRuntimeData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<GeometryGraphRuntimeData>();
                data.name = graphModel.Name;
                AssetDatabase.AddObjectToAsset(data, assetPath);
                AssetDatabase.ImportAsset(assetPath);
            }

            return data;
        }

        private static GeometryGraphResolverContext Resolve(IGraphModel graphModel)
        {
            var context = new GeometryGraphResolverContext();

            var resultNode = graphModel.NodeModels.FirstOrDefault(model => model is GraphResult) as GraphResult;

            var rootNode = resultNode.DataIn.GetConnectedPorts().FirstOrDefault().NodeModel as IGeometryNode;

            context.BeginWriteCombiner(new CombinerInstruction(CombinerOperation.Min, context.ZeroFloatProperty, context.CurrentCombinerDepth));

            rootNode.Resolve(context, context.OriginalGeometryStackData);

            context.FinishWritingCombiner();

            context.BuildBuffers();

            return context;
        }
    }
}