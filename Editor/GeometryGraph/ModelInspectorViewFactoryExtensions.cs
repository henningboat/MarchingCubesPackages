﻿using Editor.GeometryGraph.DataModel.MathNodes;
using Editor.GeometryGraph.GraphElements;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    [GraphElementsExtensionMethodsCache(typeof(ModelInspectorView))]
    public static class ModelInspectorViewFactoryExtensions
    {
        public static IModelUI CreateNodeInspector(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, MathOperator model)
        {
            var ui = new ModelInspector();
            ui.Setup(model, commandDispatcher, elementBuilder.View as ModelInspectorView, elementBuilder.Context);

            var nodeInspectorFields = NodeFieldsInspector.Create("node-fields", model, ui, ModelInspector.ussClassName);
            ui.PartList.AppendPart(nodeInspectorFields);

            var mathOpInspectorFields = MathOperatorInspector.Create("mathop-fields", model, ui, ModelInspector.ussClassName);
            ui.PartList.AppendPart(mathOpInspectorFields);

            ui.BuildUI();
            ui.UpdateFromModel();

            return ui;
        }
    }
}