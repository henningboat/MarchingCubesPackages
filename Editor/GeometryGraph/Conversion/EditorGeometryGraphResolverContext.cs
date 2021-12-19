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
using henningboat.CubeMarching.PrimitiveBehaviours;
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

        public readonly GeometryGraphProperty ZeroFloatProperty;

        public CombinerState CurrentCombiner => _runtimeResolver.CurrentCombiner;
        public ExposedVariable[] ExposedVariables { get; private set; }
        public int CurrentCombinerDepth => _runtimeResolver.CurrentCombinerDepth;

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
            ZeroFloatProperty = GetOrCreateProperty(SerializableGUID.Generate(), 0.0f);
            OriginTransformation = GetOrCreateProperty(SerializableGUID.Generate(), Matrix4x4.Translate(Vector3.one*-32));
            DefaultColorFloat3 = GetOrCreateProperty(SerializableGUID.Generate(), new float3(1,1,1));
            DefaultColor = GetOrCreateProperty(SerializableGUID.Generate(), Color.blue);

            OriginalGeometryStackData = new GeometryStackData() {Color = DefaultColor, Transformation = OriginTransformation};
        }


        public void BeginWriteCombiner(CombinerInstruction combiner)
        {
            _runtimeResolver.BeginWriteCombiner(new CombinerState(combiner.Operation,combiner.blendFactorProperty));
        }

        public void FinishWritingCombiner()
        {
            _runtimeResolver.FinishWritingCombiner();
        }

        public void WriteShape(ShapeProxy shapeProxy)
        {
            _runtimeResolver.AddShape(shapeProxy);
        }

        public GeometryGraphProperty GetOrCreateProperty<T>(SerializableGUID guid, T value)
        {
            if (_properties.TryGetValue(guid, out var existingProperty))
            {
                return existingProperty;
            }
            
        
            // if (newProperty is GeometryGraphMathOperatorProperty mathOperator)
            // {
            //     throw new NotImplementedException();
            //    // _mathInstructionsBuffer.Add(mathOperator.GetMathInstruction());
            // }

            var newProperty = _runtimeResolver.CreateProperty(value);
            
            _properties[guid] = newProperty;
            return newProperty;
        }

        public void BuildBuffers()
        {
            ExposedVariables = _graphModel.VariableDeclarations.Select(CreateExposedVariable).ToArray();
            _runtimeResolver.ExportBuffers(out ValueBuffer,out  GeometryInstructions,out  MathInstructions);
        }

        public float[] ValueBuffer;
        public GeometryInstruction[] GeometryInstructions;
        public MathInstruction[] MathInstructions;

        private ExposedVariable CreateExposedVariable(IVariableDeclarationModel variable)
        {
            throw new NotImplementedException();
            // var geometryGraphProperty = _properties[variable.Guid];
            // List<float> defaultValue = new List<float>();
            //
            // for (int i = 0; i < geometryGraphProperty.GetSizeInBuffer(); i++)
            // {
            //     defaultValue.Add(_propertyValueBuffer[geometryGraphProperty.Index + i]);
            // }
            //
            // return new ExposedVariable(variable.Guid, geometryGraphProperty.Type, defaultValue.ToArray(), geometryGraphProperty.Index, variable.GetVariableName());
        }

        public void WriteDistanceModifier(DistanceModifierInstruction getDistanceModifierInstruction)
        {
            throw new NotImplementedException();
          //  _geometryInstructionBuffer.Add(getDistanceModifierInstruction.GetInstruction());
        }
        public void WritePositionModificationModifier(PositionModificationInstruction getDistanceModifierInstruction)
        {  
            throw new NotImplementedException();
            //_geometryInstructionBuffer.Add(getDistanceModifierInstruction.GetInstruction());
        }

        public void Dispose()
        {
            _runtimeResolver.Dispose();
        }

        public GeometryGraphProperty CreateMathOperation(SerializableGUID guid, GeometryGraphMathOperatorProperty geometryGraphMathOperatorProperty)
        {
            throw new NotImplementedException();
        }
    }
}