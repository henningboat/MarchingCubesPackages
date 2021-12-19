using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{    
    public class EditorGeometryGraphResolverContext:IDisposable
    {
        private Dictionary<SerializableGUID, GeometryGraphProperty> _properties = new();

        public int CurrentCombinerDepth => _runtimeResolver.CurrentCombinerDepth;

        public NativeList<GeometryInstruction> GeometryInstructionBuffer => _runtimeResolver.GeometryInstructionBuffer;

        
        public readonly GeometryGraphProperty ZeroFloatProperty;

        public CombinerInstruction CurrentCombiner => _runtimeResolver.CurrentCombiner;
        public ExposedVariable[] ExposedVariables { get; private set; }

        public GeometryGraphProperty OriginTransformation;
        public GeometryGraphProperty DefaultColor;
        public GeometryStackData OriginalGeometryStackData;
        public GeometryGraphProperty DefaultColorFloat3;
        private GeometryGraphModel _graphModel;
        private readonly RuntimeGeometryGraphResolverContext _runtimeResolver;

        public EditorGeometryGraphResolverContext(GeometryGraphModel graphModel)
        {
            _runtimeResolver = new RuntimeGeometryGraphResolverContext();
            
            _graphModel = graphModel;
            ZeroFloatProperty = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(0.0f, this, GeometryPropertyType.Float, "Zero Float Constant"));
            OriginTransformation = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(Matrix4x4.Translate(Vector3.one*-32), this, GeometryPropertyType.Float4X4, "Origin Matrix"));
            DefaultColorFloat3 = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(new Vector3(1,1,1), this, GeometryPropertyType.Float3, "Default Material Float3"));
            DefaultColor = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphMathOperatorProperty(this,GeometryPropertyType.Color32,MathOperatorType.Float3ToColor32,DefaultColorFloat3,ZeroFloatProperty, "Default Material Color"));

            OriginalGeometryStackData = new GeometryStackData() {Color = DefaultColor, Transformation = OriginTransformation};
        }


        public void BeginWriteCombiner(CombinerInstruction combiner)
        {
            _combinerStack.Push(combiner);
        }

        public void FinishWritingCombiner()
        {
            _combinerStack.Pop();

            //bit confusing: to write the combiner into it's parent, we need the parents combiner settings
            CombinerInstruction instruction = new CombinerInstruction(CurrentCombiner.Operation,
                CurrentCombiner.blendFactorProperty, CurrentCombiner.Depth + 1);
            
            _geometryInstructionBuffer.Add(instruction.GetInstruction());
        }

        public void WriteShape(ShapeType shapeType, GeometryStackData stackData, List<GeometryGraphProperty> getProperties)
        {
            var shape = new ShapeInstruction(shapeType, stackData.Transformation, stackData.Color, getProperties, CurrentCombinerDepth, CurrentCombiner);
            
            GeometryInstructionBuffer.Add(shape.GetInstruction());
        }

        public GeometryGraphProperty GetOrCreateProperty(SerializableGUID guid, GeometryGraphProperty newProperty)
        {
            if (_properties.TryGetValue(guid, out var existingProperty))
            {
                return existingProperty;
            }
            
            AddPropertyToValueBuffer(newProperty);
            
            if (newProperty is GeometryGraphMathOperatorProperty mathOperator)
            {
                _mathInstructionsBuffer.Add(mathOperator.GetMathInstruction());
            }
            
            
            _properties[guid] = newProperty;
            return newProperty;
        }

        public void BuildBuffers()
        {
            ExposedVariables = _graphModel.VariableDeclarations.Select(CreateExposedVariable).ToArray();
        }

        private void AddPropertyToValueBuffer(GeometryGraphProperty property)
        {
            property.Index = _propertyValueBuffer.Length;
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

        private ExposedVariable CreateExposedVariable(IVariableDeclarationModel variable)
        {
            var geometryGraphProperty = _properties[variable.Guid];
            List<float> defaultValue = new List<float>();

            for (int i = 0; i < geometryGraphProperty.GetSizeInBuffer(); i++)
            {
                defaultValue.Add(_propertyValueBuffer[geometryGraphProperty.Index + i]);
            }

            return new ExposedVariable(variable.Guid, geometryGraphProperty.Type, defaultValue.ToArray(), geometryGraphProperty.Index, variable.GetVariableName());
        }

        public void WriteDistanceModifier(DistanceModifierInstruction getDistanceModifierInstruction)
        {
            _geometryInstructionBuffer.Add(getDistanceModifierInstruction.GetInstruction());
        }
        public void WritePositionModificationModifier(PositionModificationInstruction getDistanceModifierInstruction)
        {
            _geometryInstructionBuffer.Add(getDistanceModifierInstruction.GetInstruction());
        }

        public void Dispose()
        {
            _geometryInstructionBuffer.Dispose();
            _mathInstructionsBuffer.Dispose();
            _propertyValueBuffer.Dispose();
        }
    }
}