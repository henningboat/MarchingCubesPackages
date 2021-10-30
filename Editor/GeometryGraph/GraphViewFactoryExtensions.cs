using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.GeometryGraph.Editor.GraphElements;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    [GraphElementsExtensionMethodsCache(typeof(GraphView))]
    public static class GraphViewFactoryExtensions
    {
        public static IModelUI CreateNode(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, MathOperator model)
        {
            IModelUI ui = new VariableInputNode();
            ui.SetupBuildAndUpdate(model, commandDispatcher, elementBuilder.View, elementBuilder.Context);
            return ui;
        }
        
        public static IModelUI CreateGeometryNodeUI(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, GeometryNodeModel geometryNodeModel)
        {
            var ui = new GeometryGraphUI();
            ui.SetupBuildAndUpdate(geometryNodeModel, commandDispatcher, elementBuilder.View, elementBuilder.Context);
            return ui;
        }

        public static IModelUI CreateGeometryGraphVariableDeclarationModelUI(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, GeometryGraphVariableDeclarationModel model)
        {
            IModelUI ui;

            if (elementBuilder.Context == BlackboardVariablePropertiesPart.blackboardVariablePropertiesPartCreationContext)
            {
                ui = new GeometryGraphBBVarPropertyView();
                ui.SetupBuildAndUpdate(model, commandDispatcher, elementBuilder.View, elementBuilder.Context);
            }
            else
            {
                ui = UnityEditor.GraphToolsFoundation.Overdrive.GraphViewFactoryExtensions.CreateVariableDeclarationModelUI(elementBuilder, commandDispatcher, model);
            }

            return ui;
        }
    }
}