﻿using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Rendering;
using TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Code.SIMDMath.SimdMath;


namespace GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct CShapeSphere : IComponentData, ITerrainModifierShape
    {
        public bool Equals(CShapeSphere other)
        {
            return radius.Equals(other.radius);
        }

        public override bool Equals(object obj)
        {
            return obj is CShapeSphere other && Equals(other);
        }

        public override int GetHashCode()
        {
            return radius.GetHashCode();
        }

        #region ActualData

        [FieldOffset(0)] public FloatValue radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            var radiusValue = radius.Resolve(valueBuffer);
            return length(positionOS) - radiusValue;
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            var radiusValue = radius.Resolve(valueBuffer);
            var center = translation.Value;
            return new TerrainBounds
            {
                min = center - radiusValue, max = center + radiusValue
            };
        }

        public uint CalculateHash()
        {
            return math.asuint(radius.Index);
        }

        public ShapeType Type => ShapeType.Sphere;
    }

    public interface IValueReference
    {
        int GetDimensions();
    }

    [Serializable]
    public struct FloatValue
    {
        public int Index;

        public float Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer[Index];
        }
    }

    [Serializable]
    public struct Float3Value
    {
        public int Index;

        public float3 Resolve(NativeArray<float> valueBuffer)
        {
            return new(valueBuffer[Index], valueBuffer[Index + 1], valueBuffer[Index + 2]);
        }
    }
    
    [Serializable]
    public struct MaterialDataValue
    {
        public int Index;

        public TerrainMaterial Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer.Reinterpret<TerrainMaterial>()[Index];
        }
    }
    
    [Serializable]
    public struct Float4X4Value
    {
        public int Index;

        public float4x4 Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer.Read<float4x4>(Index);
        }
    }
}