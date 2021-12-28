using System;
using System.Collections.Generic;
using System.Linq;
using Editor.GeometryGraph.DataModel;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using Editor.GeometryGraph.DataModel.MathNodes;
using Editor.GeometryGraph.DataModel.ShapeNodes;
using Editor.GeometryGraph.DataModel.TransformationNode;
using henningboat.CubeMarching.GeometryComponents;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Searcher;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    internal class GeometryGraphStencil : Stencil, ISearcherDatabaseProvider
    {
        private List<SearcherDatabaseBase> m_Databases = new();

        public override string ToolName => GraphName;

        public static string GraphName => "Geometry Graph";

        public GeometryGraphStencil()
        {
            SearcherItem MakeSearcherItem((Type t, string name) tuple)
            {
                return new GraphNodeModelSearcherItem(GraphModel, null, data => data.CreateNode(tuple.t), tuple.name);
            }

            SearcherItem MakeShapeSearcherItem(ShapeType shapeType)
            {
                return new GraphNodeModelSearcherItem(GraphModel, null, data => data.CreateNode(typeof(ShapeNode),null,
                    model =>
                    {
                        ((ShapeNode)model).InitializeShapeType(shapeType);
                    }), shapeType.ToString());
            }
            

            var combiners = TypeCache.GetTypesDerivedFrom(typeof(GeometryCombinerNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var combinerItems = new SearcherItem("Combiners", "", combiners.ToList());

            var transformations = TypeCache.GetTypesDerivedFrom(typeof(TransformationNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var transformationItems = new SearcherItem("Transformations", "", transformations.ToList());
            
            var distortions = TypeCache.GetTypesDerivedFrom(typeof(DistanceModificationNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var distortionItems = new SearcherItem("Distortion", "", distortions.ToList());
            
            var positionModifications = TypeCache.GetTypesDerivedFrom(typeof(PositionModificationNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var positionModificationItems = new SearcherItem("Distortion", "", positionModifications.ToList());

            var mathNodes = TypeCache.GetTypesDerivedFrom(typeof(MathNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            mathNodes.Add( MakeSearcherItem((typeof(GraphResult), "Result")));
            mathNodes.Add( MakeSearcherItem((typeof(ColorNode), "Color")));

            List<SearcherItem> shapes = new();
            foreach (var shapeType in Enum.GetValues(typeof(ShapeType)))
            {
                shapes.Add(MakeShapeSearcherItem((ShapeType) shapeType));
            }
            var shapeItems = new SearcherItem("Shapes", "", shapes.ToList());
            
            
            var mathNodeItems = new SearcherItem("MathNodes", "", mathNodes.ToList());


            var others = new List<SearcherItem>
            {
                new GraphNodeModelSearcherItem(GraphModel, null,
                    t => t.GraphModel.CreateConstantNode(TypeHandle.Float, "", t.Position, t.Guid, null, t.SpawnFlags),
                    "Constant")
            };

            var constantsItem = new SearcherItem("Values", "", others);


            var items = new List<SearcherItem> {combinerItems, shapeItems, mathNodeItems, constantsItem, transformationItems, distortionItems, positionModificationItems};

            var searcherDatabase = new SearcherDatabase(items);
            m_Databases.Add(searcherDatabase);
        }

        private void OnNodeCreated(INodeModel obj)
        {
            Debug.Log(obj.Guid);
        }

        public override IToolbarProvider GetToolbarProvider()
        {
            return new GeometryGraphToolbarProvider();
        }

        public override void OnGraphProcessingStarted(IGraphModel graphModel)
        {
            Debug.Log("Graph processing started");
        }

        public override Type GetConstantNodeValueType(TypeHandle typeHandle)
        {
            return TypeToConstantMapper.GetConstantNodeType(typeHandle);
        }

        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
        {
            return this;
        }

        public override IGraphProcessor CreateGraphProcessor()
        {
            return new GeometryGraphProcessor();
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetGraphElementsSearcherDatabases(IGraphModel graphModel)
        {
            return m_Databases;
        }

        private List<SearcherDatabaseBase> m_EmptyList = new();

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetVariableTypesSearcherDatabases()
        {
            return m_EmptyList;
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetGraphVariablesSearcherDatabases(IGraphModel graphModel)
        {
            return m_Databases;
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetDynamicSearcherDatabases(IPortModel portModel)
        {
            return m_Databases;
        }

        public List<SearcherDatabaseBase> GetDynamicSearcherDatabases(IEnumerable<IPortModel> portModel)
        {
            return m_Databases;
        }

        /// <inheritdoc />
        public override IBlackboardGraphModel CreateBlackboardGraphModel(IGraphAssetModel graphAssetModel)
        {
            return new BlackboardGraphModel(graphAssetModel);
        }

        public override void PopulateBlackboardCreateMenu(string sectionName, GenericMenu menu, CommandDispatcher commandDispatcher)
        {
            menu.AddItem(new GUIContent("Create Variable"), false, () =>
            {
                const string newItemName = "variable";
                var finalName = newItemName;
                var i = 0;
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = newItemName + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, TypeHandle.Float, typeof(GeometryGraphVariableDeclarationModel)));
            });

            menu.AddItem(new GUIContent("Create Vector3"), false, () =>
            {
                const string newItemName = "variable";
                var finalName = newItemName;
                var i = 0;
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = newItemName + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, TypeHandle.Vector3, typeof(GeometryGraphVariableDeclarationModel)));
            });
        }

        public override bool GetPortCapacity(IPortModel portModel, out PortCapacity capacity)
        {
            if (portModel.DataTypeHandle == TypeHandle.ExecutionFlow)
            {
                if ((portModel.Direction & PortDirection.Input) != 0)
                {
                    capacity = PortCapacity.Single;
                    return true;
                }
            }

            capacity = default;
            return false;
        }
    }

    internal class GenericNodeTest<T> : NodeModel
    {
        protected override void OnDefineNode()
        {
             this.AddExecutionOutputPort("","InTest", PortOrientation.Vertical);
        }
    }
}