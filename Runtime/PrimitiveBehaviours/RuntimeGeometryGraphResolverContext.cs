using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Random = UnityEngine.Random;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public  class CombinerState
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty BlendValue;

        public CombinerState(CombinerOperation operation, GeometryGraphProperty blendValue)
        {
            Operation = operation;
            BlendValue = blendValue;
        }

    }
    public class RuntimeGeometryGraphResolverContext : IDisposable
    {
        public int CurrentCombinerDepth => _combinerStack.Count - 1;
        private NativeList<float> _propertyValueBuffer;
        private NativeList<GeometryInstruction> _geometryInstructionBuffer;
        private NativeList<MathInstruction> _mathInstructionsBuffer;

        public NativeList<GeometryInstruction> GeometryInstructionBuffer => _geometryInstructionBuffer;

        private Stack<CombinerState> _combinerStack = new();
        public readonly GeometryGraphProperty ZeroFloatProperty;
        private List<ExposedVariable> _exposedProperties=new();

        public CombinerState CurrentCombiner => _combinerStack.Peek();
        public CombinerOperation CurrentCombinerOperation { get; }

        public GeometryGraphProperty OriginTransformation { get; }
        public GeometryGraphProperty DefaultColor{ get; }
        public GeometryGraphProperty DefaultColorFloat3{ get; }

        public RuntimeGeometryGraphResolverContext()
        {
            _geometryInstructionBuffer = new NativeList<GeometryInstruction>(Allocator.Temp);
            _mathInstructionsBuffer = new NativeList<MathInstruction>(Allocator.Temp);
            _propertyValueBuffer = new NativeList<float>(Allocator.Temp);

            OriginTransformation = Constant(Matrix4x4.identity);
            DefaultColor = Constant(0);
            ZeroFloatProperty = Constant(0);
            DefaultColorFloat3 = Constant(float3.zero);

            _combinerStack.Push(new CombinerState(CombinerOperation.Min, ZeroFloatProperty));
        }


        public void BeginWriteCombiner(CombinerState combiner)
        {
            _combinerStack.Push(combiner);
        }

        public void FinishWritingCombiner()
        {
            //bit confusing: to write the combiner into it's parent, we need the parents combiner settings
            _combinerStack.Pop();

            _geometryInstructionBuffer.Add(GeometryInstructionUtility.CreateInstruction(
                GeometryInstructionType.Combiner, (int) CurrentCombiner.Operation, CurrentCombinerDepth,
                CurrentCombinerOperation, DefaultColor, OriginTransformation, new List<GeometryGraphProperty>(),
                DefaultColor));
        }

        public void Dispose()
        {
            _geometryInstructionBuffer.Dispose();
            _mathInstructionsBuffer.Dispose();
            _propertyValueBuffer.Dispose();
        }

        public GeometryGraphProperty CreateProperty<T>(T value, string debugInfo = "")
        {
            switch (value)
            {
                case float floatValue:
                    return Constant(floatValue);
                case float3 float3Value:
                    return Constant(float3Value);
                case Vector3 float3Value:
                    return Constant(float3Value);
                case Matrix4x4 matrixValue:
                    return Constant(matrixValue);
                case Color _:
                    return Constant(0);
                default:
                    throw new Exception(typeof(T) + " is not supported");
            }
        }
        
        private GeometryGraphProperty Constant(float constant)
        {
            var reference = new GeometryGraphConstantProperty(_propertyValueBuffer.Length ,constant,GeometryPropertyType.Float);
            _propertyValueBuffer.Add(constant);
            return reference;
        }

        private GeometryGraphProperty Constant(float3 constant)
        {
            var reference = new GeometryGraphConstantProperty(_propertyValueBuffer.Length, constant,GeometryPropertyType.Float3);
            _propertyValueBuffer.Add(constant.x);
            _propertyValueBuffer.Add(constant.y);
            _propertyValueBuffer.Add(constant.z);
            return reference;
        }

        private GeometryGraphProperty Constant(Matrix4x4 constant)
        {
            var reference = new GeometryGraphConstantProperty(_propertyValueBuffer.Length,constant, GeometryPropertyType.Float4X4);
            var constantFloat4X4Value = constant;
            for (var i = 0; i < 16; i++)
            {
                _propertyValueBuffer.Add(constantFloat4X4Value[i]);
            }

            return reference;
        }

        public void AddShape(GeometryInstructionProxy sphereShapeProxy)
        {
            _geometryInstructionBuffer.Add(sphereShapeProxy.GetGeometryInstruction(this));
        }

        public NewGeometryGraphData GetGeometryGraphData()
        {
            var hash = new Hash128();
            hash.Append(_geometryInstructionBuffer.ToArray());
            hash.Append(_mathInstructionsBuffer.ToArray());
            hash.Append(_geometryInstructionBuffer.ToArray());
            return NewGeometryGraphData.InitializeData(_propertyValueBuffer.ToArray(),
                _mathInstructionsBuffer.ToArray(), _geometryInstructionBuffer.ToArray(), hash,
                new Float4X4Value() {Index = OriginTransformation.Index},
                _exposedProperties.ToArray());
        }

        public void ExportBuffers(out float[] values, out GeometryInstruction[] geometryInstructions, out MathInstruction[] mathInstructions)
        {
            values = _propertyValueBuffer.ToArray();
            geometryInstructions = _geometryInstructionBuffer.ToArray();
            mathInstructions = _mathInstructionsBuffer.ToArray();
        }

        public void AddMathInstruction(MathInstruction mathInstruction)
        {
            _mathInstructionsBuffer.Add(mathInstruction);
        }

        public GeometryGraphProperty CreateOrGetExposedProperty<T>(SerializableGUID serializableGuid, T  value)
        {
            foreach (var variable in _exposedProperties)
            {
                if (variable.ID == serializableGuid)
                {
                    return new GeometryGraphConstantProperty(variable.IndexInValueBuffer, value, variable.Type, variable.Name);
                }
            }

            var property = CreateProperty(value);

            var exposedVariable = new ExposedVariable(serializableGuid, GeometryPropertyType.Float,
                new []{10.0f}, property.Index, "");
            _exposedProperties.Add(exposedVariable);
            
            return exposedVariable;
        }
    }
}