using System;
using System.Collections.Generic;
using Editor.GeometryGraph.GraphElements;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.Serialization;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    [Serializable]
    public class CopyGeometryLayerNodeModel : GeometryNodeModel, IGeometryNode
    {
        [FormerlySerializedAs("_sourceLayer")] [SerializeField]
        private GeometryLayerAsset sourceLayerAsset;

        public GeometryLayerAsset SourceLayerAsset
        {
            get => sourceLayerAsset;
            set => sourceLayerAsset = value;
        }

        protected override void OnDefineNode()
        {
            AddExecutionOutput("GeometryOut");
        }

        public void Resolve(GeometryInstructionListBuilder context)
        {
            var geometryInstruction = GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.CopyLayer, 0,
                new List<GeometryGraphProperty>());
            geometryInstruction.SourceLayerID = sourceLayerAsset.GeometryLayer.ID;
            context.WriteInstruction(geometryInstruction);
        }
    }


    internal class CopyGeometryLayerNode : CollapsibleInOutNode
    {
        public static readonly string paramContainerPartName = "parameter-container";

        protected override void BuildPartList()
        {
            base.BuildPartList();

            PartList.InsertPartAfter(titleIconContainerPartName,
                TemperatureAndTimePart.Create(paramContainerPartName, Model, this, ussClassName));
        }
    }

    [GraphElementsExtensionMethodsCache(typeof(GraphView))]
    public static class GeometryLayerGraphViewExtension
    {
        public static IModelUI CreateGeometryNodeUI(this ElementBuilder elementBuilder,
            CommandDispatcher commandDispatcher, CopyGeometryLayerNodeModel model)
        {
            var ui = new CopyGeometryLayerNode();
            ui.SetupBuildAndUpdate(model, commandDispatcher, elementBuilder.View, elementBuilder.Context);
            return ui;
        }
    }


    public class SetGeometryLayerCommand : ModelCommand<CopyGeometryLayerNodeModel, GeometryLayerAsset>
    {
        private const string k_UndoStringSingular = "Set GeometryLayer";
        private const string k_UndoStringPlural = "Set GeometryLayers";

        public SetGeometryLayerCommand(CopyGeometryLayerNodeModel[] nodes, GeometryLayerAsset value)
            : base(k_UndoStringSingular, k_UndoStringPlural, value, nodes)
        {
        }

        public static void DefaultHandler(GraphToolState graphToolState, SetGeometryLayerCommand command)
        {
            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.SourceLayerAsset = command.Value;
                    graphUpdater.MarkChanged(command.Models);
                }
            }
        }
    }
}