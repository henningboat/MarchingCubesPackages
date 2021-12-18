using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public  class CombinerState
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphValue BlendValue;

        public CombinerState(CombinerOperation operation, GeometryGraphValue blendValue)
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
        public readonly GeometryGraphValue ZeroFloatProperty;

        public CombinerState CurrentCombiner => _combinerStack.Peek();
        public ExposedVariable[] ExposedVariables { get; private set; }
        public CombinerOperation CurrentCombinerOperation { get; }

        public GeometryGraphValue OriginTransformation;
        public GeometryGraphValue DefaultColor;
        public GeometryGraphValue DefaultColorFloat3;

        public RuntimeGeometryGraphResolverContext()
        {
            _geometryInstructionBuffer = new NativeList<GeometryInstruction>(Allocator.Temp);
            _mathInstructionsBuffer = new NativeList<MathInstruction>(Allocator.Temp);
            _propertyValueBuffer = new NativeList<float>(Allocator.Temp);

            OriginTransformation = Constant(Matrix4x4.identity);
            DefaultColor = Constant(0);
            DefaultColorFloat3 = Constant(float3.zero);
        }


        public void BeginWriteCombiner(CombinerState combiner)
        {
            _combinerStack.Push(combiner);
        }

        public void FinishWritingCombiner()
        {
            //bit confusing: to write the combiner into it's parent, we need the parents combiner settings
            CombinerState state = new CombinerState(CurrentCombiner.Operation,
                CurrentCombiner.BlendValue);

            _geometryInstructionBuffer.Add(GeometryInstructionUtility.CreateInstruction(
                GeometryInstructionType.Combiner, (int) CurrentCombiner.Operation, CurrentCombinerDepth,
                CurrentCombinerOperation, DefaultColor, OriginTransformation, new List<GeometryGraphValue>(),
                DefaultColor));
        }

        public void Dispose()
        {
            _geometryInstructionBuffer.Dispose();
            _mathInstructionsBuffer.Dispose();
            _propertyValueBuffer.Dispose();
        }

        public GeometryGraphValue Constant(float constant)
        {
            var reference = new GeometryGraphValue(_propertyValueBuffer.Length,1 );
            _propertyValueBuffer.Add(constant);
            return reference;
        }

        public GeometryGraphValue Constant(float3 constant)
        {
            var reference = new GeometryGraphValue(_propertyValueBuffer.Length,3);
            _propertyValueBuffer.Add(constant.x);
            _propertyValueBuffer.Add(constant.y);
            _propertyValueBuffer.Add(constant.z);
            return reference;
        }

        public GeometryGraphValue Constant(Matrix4x4 constant)
        {
            var reference = new GeometryGraphValue(_propertyValueBuffer.Length,16);
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
    }

    public class GeometryGraphValue
    {
        public readonly int Index;
        public int Size;

        public GeometryGraphValue(int index, int size)
        {
            Size = size;
            Index = index;
        }
    }
}