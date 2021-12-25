using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Random = UnityEngine.Random;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class CombinerState
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

        public CombinerState CurrentCombiner => _combinerStack.Peek();
        public CombinerOperation CurrentCombinerOperation { get; }

        public GeometryGraphProperty OriginTransformation { get; }
        public GeometryGraphProperty DefaultColor { get; }
        public GeometryGraphProperty DefaultColorFloat3 { get; }

        public List<GeometryGraphProperty> _exposedVariables;

        public RuntimeGeometryGraphResolverContext()
        {
            _geometryInstructionBuffer = new NativeList<GeometryInstruction>(Allocator.Temp);
            _mathInstructionsBuffer = new NativeList<MathInstruction>(Allocator.Temp);
            _propertyValueBuffer = new NativeList<float>(Allocator.Temp);

            _exposedVariables = new List<GeometryGraphProperty>();

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

        private float32 GetFloat32(float constant)
        {
            var value = new float32
            {
                [0] = constant
            };
            return value;
        }

        private float32 GetFloat32(float3 constant)
        {
            var value = new float32
            {
                [0] = constant.x,
                [1] = constant.y,
                [2] = constant.z
            };
            return value;
        }

        private float32 GetFloat32(Matrix4x4 constant)
        {
            var value = new float32();

            for (var i = 0; i < 16; i++) value[i] = constant[i];

            return value;
        }

        private GeometryGraphProperty Constant(float constant)
        {
            var valueContainer = GetFloat32(constant);
            const GeometryPropertyType type = GeometryPropertyType.Float;
            return GetGeometryGraphProperty(default, type, valueContainer, "",
                $"Constant {type.ToString()} {constant}");
        }


        private GeometryGraphProperty Constant(float3 constant)
        {
            var valueContainer = GetFloat32(constant);
            const GeometryPropertyType type = GeometryPropertyType.Float3;
            return GetGeometryGraphProperty(default, type, valueContainer, "",
                $"Constant {type.ToString()} {constant}");
        }


        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float value)
        {
            var valueContainer = GetFloat32(value);
            const GeometryPropertyType type = GeometryPropertyType.Float;
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float3 value)
        {
            const GeometryPropertyType type = GeometryPropertyType.Float3;
            if (TryGetExisting(serializableGuid,type, out GeometryGraphProperty existing))
            {
                return existing;
            }

            var valueContainer = GetFloat32(value);
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        private bool TryGetExisting(SerializableGUID id, GeometryPropertyType type, out GeometryGraphProperty geometryGraphProperty)
        {
            foreach (var exposedVariable in _exposedVariables)
            {
                if (exposedVariable.ID == id)
                {
                    if (exposedVariable.Type != type)
                    {
                        throw new ArgumentException(
                            $"You requested a variable with type {type}. A variable with GUID {id.ToString()} exists, but it it has the type {exposedVariable.Type}");
                    }

                    geometryGraphProperty=exposedVariable;
                    return true;
                }
            }

            geometryGraphProperty = default;
            return false;
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float4x4 value)
        {
            var valueContainer = GetFloat32(value);
            const GeometryPropertyType type = GeometryPropertyType.Float4X4;
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }


        private GeometryGraphProperty Constant(Matrix4x4 constant)
        {
            var valueContainer = GetFloat32(constant);
            const GeometryPropertyType type = GeometryPropertyType.Float4X4;
            return GetGeometryGraphProperty(default, type, valueContainer, "",
                $"Constant {type.ToString()} {constant}");
        }

        public GeometryGraphProperty GetGeometryGraphProperty(SerializableGUID id, GeometryPropertyType type,
            float32 value, string name, string debugInformation)
        {
            var index = _propertyValueBuffer.Length;

            var geometryGraphProperty = new GeometryGraphProperty(index, id, type, value, name, debugInformation);

            AddToValueBuffer(value, geometryGraphProperty);

            return geometryGraphProperty;
        }

        private void AddToValueBuffer(float32 value, GeometryGraphProperty property)
        {
            for (var i = 0; i < property.GetSizeInBuffer(); i++) _propertyValueBuffer.Add(value[i]);
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
                OriginTransformation,_exposedVariables.ToArray());
        }

        public void ExportBuffers(out float[] values, out GeometryInstruction[] geometryInstructions,
            out MathInstruction[] mathInstructions)
        {
            values = _propertyValueBuffer.ToArray();
            geometryInstructions = _geometryInstructionBuffer.ToArray();
            mathInstructions = _mathInstructionsBuffer.ToArray();
        }

        public void AddMathInstruction(MathInstruction mathInstruction)
        {
            _mathInstructionsBuffer.Add(mathInstruction);
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID id, string name, GeometryPropertyType type)
        {
            if (TryGetExisting(id,type, out GeometryGraphProperty existing))
            {
                return existing;
            }

            //todo add support for default values
            float32 valueContainer = default;
            var newProperty = GetGeometryGraphProperty(id, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }
    }
}