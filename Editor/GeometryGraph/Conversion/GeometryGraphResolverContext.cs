﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryMath;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class GeometryGraphResolverContext
    {
        private Dictionary<SerializableGUID, GeometryGraphProperty> _properties = new();
        private List<GeometryGraphInstruction> _instructions = new();

        public int CurrentCombinerDepth => _combinerStack.Count - 1;
        private List<float> _propertyValueBuffer;
        private List<GeometryInstruction> _geometryInstructionBuffer;
        private List<MathInstruction> _mathInstructionsBuffer;

        public List<float> PropertyValueBuffer => _propertyValueBuffer;
        public List<MathInstruction> MathInstructionBuffer => _mathInstructionsBuffer;

        public List<GeometryInstruction> GeometryInstructionBuffer => _geometryInstructionBuffer;

        private Stack<CombinerInstruction> _combinerStack = new();
        public readonly GeometryGraphProperty ZeroFloatProperty;
        private List<GeometryTransformation> _translationsBuffer;

        public CombinerInstruction CurrentCombiner => _combinerStack.Peek();
        public GeometryGraphProperty OriginTransformation;
        public GeometryGraphProperty DefaultColor;
        public GeometryStackData OriginalGeometryStackData;
        public GeometryGraphProperty DefaultColorFloat3;

        public GeometryGraphResolverContext()
        {
            ZeroFloatProperty = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(0.0f, this, GeometryPropertyType.Float, "Zero Float Constant"));
            OriginTransformation = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(Matrix4x4.Translate(Vector3.one*-32), this, GeometryPropertyType.Float4X4, "Origin Matrix"));
            DefaultColorFloat3 = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(new Vector3(1,1,1), this, GeometryPropertyType.Float3, "Default Material Float3"));
            DefaultColor = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphMathOperatorProperty(this,GeometryPropertyType.Color32,MathOperatorType.Float3ToColor32,DefaultColorFloat3,ZeroFloatProperty, "Default Material Color"));

            OriginalGeometryStackData = new GeometryStackData() {Color = DefaultColor, Transformation = OriginTransformation};
            
            _combinerStack.Push(new CombinerInstruction(CombinerOperation.Min, ZeroFloatProperty, 0));
        }


        public void BeginWriteCombiner(CombinerInstruction combiner)
        {
            _combinerStack.Push(combiner);
        }

        public void FinishWritingCombiner()
        {
            var combinerToFinish = _combinerStack.Pop();
            _instructions.Add(combinerToFinish);
        }

        public void WriteShape(ShapeType shapeType, GeometryStackData stackData, List<GeometryGraphProperty> getProperties)
        {
            _instructions.Add(new ShapeInstruction(shapeType, stackData.Transformation, stackData.Color, getProperties, CurrentCombinerDepth, CurrentCombiner));
        }

        public GeometryGraphExposedVariableNode GetExposedVariableProperty(SerializableGUID guid)
        {
            return _properties.Values.Where(property => property is GeometryGraphExposedVariableNode).FirstOrDefault(property => ((GeometryGraphExposedVariableNode) property).Variable.Guid == guid) as
                GeometryGraphExposedVariableNode;
        }

        public GeometryGraphProperty GetOrCreateProperty(SerializableGUID guid, GeometryGraphProperty newProperty)
        {
            if (_properties.TryGetValue(guid, out var existingProperty))
            {
                return existingProperty;
            }
            else
            {
                _properties[guid] = newProperty;
                return newProperty;
            }
        }

        public void BuildBuffers()
        {
            _propertyValueBuffer = new List<float>();
            var geometryGraphProperties = _properties.Values;
            foreach (var property in geometryGraphProperties)
            {
                property.Index = _propertyValueBuffer.Count;
                switch (property.Type)
                {
                    case GeometryPropertyType.Float:
                        var constantFloatValue = property.GetValue<float>();
                        _propertyValueBuffer.Add(constantFloatValue);
                        break;
                    case GeometryPropertyType.Float3:
                        var constantFloat3Value = property.GetValue<Vector3>();
                        for (var i = 0; i < 3; i++)
                        {
                            _propertyValueBuffer.Add(constantFloat3Value[i]);
                        }

                        break;
                    case GeometryPropertyType.Float4X4:
                        var constantFloat4X4Value = property.GetValue<Matrix4x4>();
                        for (var i = 0; i < 16; i++)
                        {
                            _propertyValueBuffer.Add(constantFloat4X4Value[i]);
                        }
                        break;
                    case GeometryPropertyType.Color32:
                        //colors are always calculated by math instructions, so we don't
                        //need to write any default values
                        _propertyValueBuffer.Add(-1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _mathInstructionsBuffer = new List<MathInstruction>();
            foreach (var geometryGraphProperty in geometryGraphProperties)
            {
                if (geometryGraphProperty is GeometryGraphMathOperatorProperty mathOperator)
                {
                    _mathInstructionsBuffer.Add(mathOperator.GetMathInstruction());
                }
            }

            _geometryInstructionBuffer = new List<GeometryInstruction>();
            for (var i = 0; i < _instructions.Count; i++)
            {
                GeometryInstructionBuffer.Add(_instructions[i].GetInstruction());
            }
        }

        public void WriteDistanceModifier(DistanceModifierInstruction getDistanceModifierInstruction)
        {
            _instructions.Add(getDistanceModifierInstruction);
        }
        public void WritePositionModificationModifier(PositionModificationInstruction getDistanceModifierInstruction)
        {
            _instructions.Add(getDistanceModifierInstruction);
        }
    }

    public struct GeometryTransformation
    {
        public GeometryTransformationType Type;
        public FloatValue Value;
        public int ParentIndex;
    }

    public enum GeometryTransformationType
    {
        Translation = 1,
        EulerRotation = 2,
        Scale = 3,
        Origin
    }
}