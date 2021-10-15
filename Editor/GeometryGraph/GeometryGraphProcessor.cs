using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using GeometryComponents;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    internal class GeometryGraphProcessor : IGraphProcessor
    {
        public GraphProcessingResult ProcessGraph(IGraphModel graphModel)
        {
            try
            {
                var result = Resolve(graphModel);

                var data = GetRuntimeData(graphModel);
                data.InitializeData(result.PropertyValueBuffer, result.MathInstructionBuffer, result.GeometryInstructionBuffer);
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