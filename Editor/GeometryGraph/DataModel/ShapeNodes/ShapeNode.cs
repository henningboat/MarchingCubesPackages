using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Editor.GeometryGraph;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Mathematics;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    [Serializable]
    public class ShapeNode : GeometryNodeModel, IGeometryNode
    {
        [SerializeField] private ShapeType _shapeType;
        private List<IPortModel> _properties;
        private List<GeometryPropertyType> _typePerInputPort = new();
        public IPortModel GeometryOut { get; set; }

        public void InitializeShapeType(ShapeType shapeType)
        {
            _shapeType = shapeType;
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            Title = _shapeType.ToString();

            GeometryOut = AddExecutionOutput(nameof(GeometryOut));

            _properties = new List<IPortModel>();

            foreach (var propertyDefinition in GeometryTypeCache.GetPropertiesForType(_shapeType))
            {
                var dataInputPort =
                    this.AddDataInputPort(propertyDefinition.Item2, GetTypeHandle(propertyDefinition.Item1));
                dataInputPort.EmbeddedValue.ObjectValue = FloatArrayToObject(propertyDefinition.Item3);
                _properties.Add(dataInputPort);
                _typePerInputPort.Add(propertyDefinition.Item1);
            }

            Color = new Color(0.4f, 0, 0);
        }

        private static object FloatArrayToObject(float[] propertyDefinition)
        {
            switch (propertyDefinition.Length)
            {
                case 1:
                    return propertyDefinition[0];
                case 3:
                    return new Vector3(propertyDefinition[0], propertyDefinition[1], propertyDefinition[2]);
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyDefinition.Length), propertyDefinition,
                        "Default Values must have a length of 1 or 3");
            }
        }

        private TypeHandle GetTypeHandle(GeometryPropertyType propertyDefinitionItem1)
        {
            switch (propertyDefinitionItem1)
            {
                case GeometryPropertyType.Float:
                    return TypeHandle.Float;
                    break;
                case GeometryPropertyType.Float3:
                    return TypeHandle.Vector3;
                    break;
                case GeometryPropertyType.Float4X4:
                    return TypeHandleHelpers.GenerateTypeHandle<Matrix4x4>();
                    break;
                case GeometryPropertyType.Color32:
                    return TypeHandleHelpers.GenerateTypeHandle<Color>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyDefinitionItem1), propertyDefinitionItem1,
                        null);
            }
        }

        public void WriteGeometryInstruction(ref GeometryInstruction instruction)
        {
        }
        //
        // protected abstract ShapeProxy GetShape(RuntimeGeometryGraphResolverContext context,
        //     GeometryStackData stackData);
        //
        // public abstract List<GeometryGraphProperty> GetProperties(RuntimeGeometryGraphResolverContext context);

        public void Resolve(GeometryInstructionListBuilder context, GeometryStackData stack)
        {
            var resolvedProperties = new List<GeometryGraphProperty>();
            for (var i = 0; i < _properties.Count; i++)
                resolvedProperties.Add(_properties[i].ResolvePropertyInput(context, _typePerInputPort[i]));

            var shapeInstruction = GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape,
                (int) _shapeType, stack.Transformation, resolvedProperties);
            context.WriteInstruction(shapeInstruction);
        }
    }
}