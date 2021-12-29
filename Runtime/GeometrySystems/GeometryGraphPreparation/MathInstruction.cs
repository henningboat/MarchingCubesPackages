using System;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation
{
    [Serializable]
    public struct MathInstruction
    {
        public MathOperatorType MathOperationType;
        public int InputAIndex;
        public GeometryPropertyType InputAType;
        public int InputBIndex;
        public GeometryPropertyType InputBType;
        public int ResultIndex;
        public GeometryPropertyType ResultType;

        public MathInstruction(MathOperatorType mathOperatorType,
            GeometryGraphProperty a, GeometryGraphProperty b, GeometryGraphProperty result)
        {
            MathOperationType = mathOperatorType;
            InputAIndex = a.Index;
            InputAType = a.Type;
            
            InputBIndex = b.Index;
            InputBType = b.Type;
            
            ResultIndex = result.Index;
            ResultType = result.Type;
        }

        public void Execute(NativeArray<float> instancePropertyBuffer)
        {
            unsafe
            {
                switch (ResultType)
                {
                    case GeometryPropertyType.Float:
                        var floatResult = CalculateFloat(instancePropertyBuffer);
                        instancePropertyBuffer.Write(floatResult, ResultIndex);
                        break;
                    case GeometryPropertyType.Float3:
                        var float3Result = CalculateFloat3(instancePropertyBuffer);
                        instancePropertyBuffer.Write(float3Result, ResultIndex);
                        break;
                    case GeometryPropertyType.Float4X4:
                        float4x4 float4X4Result = CalculateFloat4X4(instancePropertyBuffer);
                        instancePropertyBuffer.Write(float4X4Result, ResultIndex);
                        break;
                    case GeometryPropertyType.Color32:
                        float color32AsFloat = CalculateColor32(instancePropertyBuffer);
                        instancePropertyBuffer.Write(color32AsFloat, ResultIndex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private float CalculateColor32(NativeArray<float> instancePropertyBuffer)
        {
            unsafe
            {
                float3 colorValue = instancePropertyBuffer.Read<float3>(InputAIndex);
                colorValue *= 255;
                byte r = (byte) colorValue.x;
                byte g = (byte) colorValue.y;
                byte b = (byte) colorValue.z;

                Color32 result = new Color32(r, g, b, 0);
                return UnsafeUtility.ReadArrayElement<float>(UnsafeUtility.AddressOf(ref result), 0);
            }
        }

        private float3 CalculateFloat3(NativeArray<float> instancePropertyBuffer)
        {
            throw new NotImplementedException();
        }

        private float4x4 CalculateFloat4X4(NativeArray<float> instancePropertyBuffer)
        {
            switch (MathOperationType)
            {
                case MathOperatorType.Translate:
                    float4x4 transformation = instancePropertyBuffer.Read<float4x4>(InputAIndex);
                    float3 offset = instancePropertyBuffer.Read<float3>(InputBIndex);
                    
                    //kind of unintuitive, but the transformation we are calculating is actually worldToObject, 
                    //so we need to invert the offset
                    var worldToLocal = math.inverse(float4x4.Translate(offset));
                    
                    return math.mul(worldToLocal, transformation);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float CalculateFloat(NativeArray<float> instancePropertyBuffer)
        {
            var inputA = instancePropertyBuffer.Read<float>(InputAIndex);
            var inputB = instancePropertyBuffer.Read<float>(InputBIndex);

            switch (MathOperationType)
            {
                case MathOperatorType.Addition:
                    return inputA + inputB;
                    break;
                case MathOperatorType.Subtraction:
                    return inputA - inputB;
                    break;
                case MathOperatorType.Multiplication:
                    return inputA * inputB;
                    break;
                case MathOperatorType.Division:
                    return inputA / inputB;
                case MathOperatorType.Min:
                    return math.min(inputA, inputB);
                    break;
                case MathOperatorType.Max:
                    return math.max(inputA, inputB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}