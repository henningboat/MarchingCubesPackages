using System;
using System.Collections.Generic;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.ShapeNodes
{
    [Serializable]
    public abstract class GeometryInstructionBaseNode<T> : GeometryNodeModel, IGeometryNode where T : Enum
    {
        [SerializeField] protected T _typeEnumValue;

        protected abstract int GetTypeEnumValue();

        private List<IPortModel> _properties;
        private List<GeometryPropertyType> _typePerInputPort = new();
        public IPortModel GeometryOut { get; set; }


        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            Title = _typeEnumValue.ToString();

            GeometryOut = AddExecutionOutput(nameof(GeometryOut));

            _properties = new List<IPortModel>();

            foreach (var propertyDefinition in GeometryTypeCache.GetPropertiesForType(_typeEnumValue))
            {
                var dataInputPort =
                    this.AddDataInputPort(propertyDefinition.Item2, GetTypeHandle(propertyDefinition.Item1), null,
                        PortOrientation.Horizontal, PortModelOptions.Default,
                        constant => { FloatArrayToObject(propertyDefinition.Item3); });

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

        public virtual void Resolve(GeometryInstructionListBuilder context)
        {
            var resolvedProperties = new List<GeometryGraphProperty>();
            for (var i = 0; i < _properties.Count; i++)
                resolvedProperties.Add(_properties[i].ResolvePropertyInput(context, _typePerInputPort[i]));

            var shapeInstruction = GeometryInstructionUtility.CreateInstruction(InstructionType,
                GetTypeEnumValue(), resolvedProperties);
            context.WriteInstruction(shapeInstruction);
        }

        public abstract GeometryInstructionType InstructionType { get; }

        public void InitializeType(T type)
        {
            _typeEnumValue = type;
        }
    }
}