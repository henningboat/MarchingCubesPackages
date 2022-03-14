using System;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public enum GeometryPropertyType
    {
        Float,
        Float3,
        Float4X4,
        Color32,
        SDF2D
    }

    public static class GeometryPropertyTypeExtensions
    {
        public static GeometryPropertyType GetGeometryPropertyTypeFromType(this Type type)
        {
            if (type == typeof(float)) return GeometryPropertyType.Float;
            if (type == typeof(float3)) return GeometryPropertyType.Float3;
            if (type == typeof(Color32)) return GeometryPropertyType.Color32;

            if (type == typeof(float4x4)) return GeometryPropertyType.Float4X4;

            if (type == typeof(SDF2DAssetReference)) return GeometryPropertyType.SDF2D;
            
            throw new ArgumentOutOfRangeException(type.ToString());
        }
    }
}