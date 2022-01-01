using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    //should be renamed
    public class GeometryInstructionListBuilder : IDisposable
    {
        public int CurrentCombinerDepth => _combinerStack.Count - 1;
        private NativeList<float> _propertyValueBuffer;
        private NativeList<GeometryInstruction> _geometryInstructionBuffer;
        private NativeList<MathInstruction> _mathInstructionsBuffer;

        private Stack<CombinerState> _combinerStack = new();
        public readonly GeometryGraphProperty ZeroFloatProperty;

        public CombinerState CurrentCombiner => _combinerStack.Peek();

        public GeometryGraphProperty OriginTransformation { get; }
        public GeometryGraphProperty DefaultColor { get; }
        public GeometryGraphProperty DefaultColorFloat3 { get; }
        public GeometryGraphProperty CurrentTransformation => _transformationStack.Peek();

        public List<GeometryGraphProperty> _exposedVariables;
        private Stack<GeometryGraphProperty> _transformationStack;

        public GeometryInstructionListBuilder()
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
            _transformationStack = new Stack<GeometryGraphProperty>();
            _transformationStack.Push(OriginTransformation);
        }

        public void BeginWriteCombiner(CombinerOperation combinerOperation, GeometryGraphProperty blendValue)
        {
            _combinerStack.Push(new CombinerState(combinerOperation, blendValue));
        }

        public void FinishWritingCombiner()
        {
            //bit confusing: to write the combiner into it's parent, we need the parents combiner settings
            _combinerStack.Pop();

            var newInstruction =
                GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Combiner, 0, null);
            GeometryInstructionUtility.AddAdditionalData(ref newInstruction, CurrentCombinerDepth,
                CurrentCombiner.Operation, CurrentCombiner.BlendValue, OriginTransformation, DefaultColor);

            WriteInstruction(newInstruction);
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

        public GeometryGraphProperty CreateEmptyProperty(GeometryPropertyType propertyType, string debugInfo = "")
        {
            switch (propertyType)
            {
                case GeometryPropertyType.Float:
                    return Constant(0.0f);
                case GeometryPropertyType.Float3:
                    return Constant(float3.zero);
                case GeometryPropertyType.Float4X4:
                    return Constant(float4x4.identity);
                case GeometryPropertyType.Color32:
                    return Constant(0.0f);
            }

            throw new InvalidOperationException();
        }

        public GeometryGraphProperty CreateOrGetExposedPropertyFromObject(SerializableGUID serializableGuid,
            string name,
            object value)
        {
            switch (value)
            {
                case float floatValue:
                    return CreateOrGetExposedProperty(serializableGuid, name, floatValue);
                case float3 float3Value:
                    return CreateOrGetExposedProperty(serializableGuid, name, float3Value);
                case Vector3 float3Value:
                    return CreateOrGetExposedProperty(serializableGuid, name, (float3) float3Value);
                case Matrix4x4 matrixValue:
                    return CreateOrGetExposedProperty(serializableGuid, name, (float4x4) matrixValue);
                case Color32 color32:
                    throw new NotImplementedException();
                // return CreateOrGetExposedPropertyFromObject(serializableGuid,name,color32);
                case Color _:
                    return Constant(0);
                default:
                    throw new Exception(value.GetType() + " is not supported");
            }
        }

        private GeometryGraphProperty Constant(float constant)
        {
            var valueContainer = float32.FromFloat(constant);
            const GeometryPropertyType type = GeometryPropertyType.Float;
            return GetGeometryGraphProperty(default, type, valueContainer, "",
                $"Constant {type.ToString()} {constant}");
        }


        private GeometryGraphProperty Constant(float3 constant)
        {
            var valueContainer = float32.FromFloat3(constant);
            const GeometryPropertyType type = GeometryPropertyType.Float3;
            return GetGeometryGraphProperty(default, type, valueContainer, "",
                $"Constant {type.ToString()} {constant}");
        }


        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float value)
        {
            var valueContainer = float32.FromFloat(value);
            const GeometryPropertyType type = GeometryPropertyType.Float;
            if (TryGetExisting(serializableGuid, type, out var existing)) return existing;
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name,
                $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float3 value)
        {
            const GeometryPropertyType type = GeometryPropertyType.Float3;
            if (TryGetExisting(serializableGuid, type, out var existing)) return existing;

            var valueContainer = float32.FromFloat3(value);
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name,
                $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        private bool TryGetExisting(SerializableGUID id, GeometryPropertyType type,
            out GeometryGraphProperty geometryGraphProperty)
        {
            foreach (var exposedVariable in _exposedVariables)
                if (exposedVariable.ID == id)
                {
                    if (exposedVariable.Type != type)
                        throw new ArgumentException(
                            $"You requested a variable with type {type}. A variable with GUID {id.ToString()} exists, but it it has the type {exposedVariable.Type}");

                    geometryGraphProperty = exposedVariable;
                    return true;
                }

            geometryGraphProperty = default;
            return false;
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID serializableGuid, string name,
            float4x4 value)
        {
            const GeometryPropertyType type = GeometryPropertyType.Float4X4;
            if (TryGetExisting(serializableGuid, type, out var existing)) return existing;
            var valueContainer = float32.FromFloat4x4(value);
            var newProperty = GetGeometryGraphProperty(serializableGuid, type, valueContainer, name,
                $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }


        private GeometryGraphProperty Constant(Matrix4x4 constant)
        {
            var valueContainer = float32.FromFloat4x4(constant);
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

        public void WriteInstruction(GeometryInstruction instruction)
        {
            GeometryInstructionUtility.AddAdditionalData(ref instruction, CurrentCombinerDepth,
                CurrentCombiner.Operation, CurrentCombiner.BlendValue, CurrentTransformation, DefaultColor);
            _geometryInstructionBuffer.Add(instruction);
        }

        public GeometryInstructionList GetGeometryGraphData()
        {
            if (_transformationStack.Count != 1)
                throw new Exception(
                    $"Transformation stack has a count of {_transformationStack.Count}, it must be 1 ones you finish building the GeometryList");

            if (_combinerStack.Count != 1)
                throw new Exception(
                    $"Combiner stack has a count of {_combinerStack.Count}, it must be 1 ones you finish building the GeometryList");

            var hash = new Hash128();
            hash.Append(_geometryInstructionBuffer.ToArray());
            hash.Append(_mathInstructionsBuffer.ToArray());
            hash.Append(_propertyValueBuffer.ToArray());
            return GeometryInstructionList.InitializeData(_propertyValueBuffer.ToArray(),
                _mathInstructionsBuffer.ToArray(), _geometryInstructionBuffer.ToArray(), hash,
                OriginTransformation, _exposedVariables.ToArray());
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID id, string name,
            GeometryPropertyType type)
        {
            if (TryGetExisting(id, type, out var existing)) return existing;

            //todo add support for default values
            float32 valueContainer = default;
            var newProperty = GetGeometryGraphProperty(id, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        public GeometryGraphProperty CreateOrGetExposedProperty(SerializableGUID id, string name,
            GeometryPropertyType type, float32 defaultValue)
        {
            if (TryGetExisting(id, type, out var existing)) return existing;

            //todo add support for default values
            var valueContainer = defaultValue;
            var newProperty = GetGeometryGraphProperty(id, type, valueContainer, name, $"{name} {type.ToString()}");
            _exposedVariables.Add(newProperty);
            return newProperty;
        }

        public void AddMathInstruction(MathOperatorType mathOperatorType, GeometryPropertyType resultType,
            GeometryGraphProperty a, GeometryGraphProperty b, out GeometryGraphProperty resultProperty)
        {
            resultProperty = CreateEmptyProperty(resultType, "Math Operation Result");
            _mathInstructionsBuffer.Add(new MathInstruction(mathOperatorType, a, b, resultProperty));
        }

        public void PushTransformation(GeometryGraphProperty newTransformation, bool relativeToParent)
        {
            newTransformation.AssertType(GeometryPropertyType.Float4X4);
            if (relativeToParent == true)
            {
                AddMathInstruction(MathOperatorType.Multiplication, GeometryPropertyType.Float4X4,
                    CurrentTransformation, newTransformation, out GeometryGraphProperty transformationRelativeToParent);
                _transformationStack.Push(transformationRelativeToParent);
            }
            else
            {
                
                _transformationStack.Push(newTransformation);
            }

        }

        public void PopTransformation()
        {
            _transformationStack.Pop();
        }
    }
}