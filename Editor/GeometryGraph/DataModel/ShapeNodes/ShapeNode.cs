﻿using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Editor.GeometryGraph;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    [Serializable]
    public class ShapeNode : GeometryNodeModel, IGeometryNode 
    {
        [SerializeField]private ShapeType _shapeType;
        private List<IPortModel> _properties;
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
                _properties.Add(this.AddDataInputPort(propertyDefinition.Item2, GetTypeHandle(propertyDefinition.Item1)));
            }

            Color = new Color(0.4f, 0, 0);
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
                    throw new ArgumentOutOfRangeException(nameof(propertyDefinitionItem1), propertyDefinitionItem1, null);
            }   
        }

        public void WriteGeometryInstruction(ref GeometryInstruction instruction)
        {
        }
        //
        // protected abstract ShapeProxy GetShape(EditorGeometryGraphResolverContext context,
        //     GeometryStackData stackData);
        //
        // public abstract List<GeometryGraphProperty> GetProperties(EditorGeometryGraphResolverContext context);

        public void Resolve(EditorGeometryGraphResolverContext context, GeometryStackData stack)
        {
            foreach (var portModel in _properties)
            {
                Debug.Log(portModel.UniqueName);
            }
          //  context.WriteShape(GetShape(context, stack));
        }
    }
}