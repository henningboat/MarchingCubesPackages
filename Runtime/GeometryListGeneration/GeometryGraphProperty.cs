﻿using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    [Serializable]
    public sealed class GeometryGraphProperty
    {
        public int Index;
        public SerializableGUID ID;
        public string DebugInformation;
        public GeometryPropertyType Type;
        public float32 DefaultValue;
        public Object DefaultAsset;

        public bool IsExposed => ID != default;
        public bool IsAsset => Type == GeometryPropertyType.SDF2D;

        public int GetSizeInBuffer()
        {
            return Type switch
            {
                GeometryPropertyType.Float => 1,
                GeometryPropertyType.Float3 => 3,
                GeometryPropertyType.Color32 => 1,
                GeometryPropertyType.Float4X4 => 16,
                GeometryPropertyType.SDF2D => 1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public GeometryGraphProperty(int index, SerializableGUID id, GeometryPropertyType type, float32 value,
            string name, string debugInformation)
        {
            ID = id;
            Index = index;
            Type = type;
            DebugInformation = debugInformation;
            DefaultValue = value;
            Name = name;
        }
        
        public GeometryGraphProperty(int index, SerializableGUID id, GeometryPropertyType type, Object value,
            string name, string debugInformation)
        {
            ID = id;
            Index = index;
            Type = type;
            DebugInformation = debugInformation;
            DefaultAsset = value;
            Name = name;
        }

        public string Name;

        public override string ToString()
        {
            return DebugInformation;
        }

        public void AssertType(GeometryPropertyType expectedType)
        {
            if (Type != expectedType)
                throw new ArgumentException($"Property must be of type {expectedType} to be valid at this point");
        }
    }
}