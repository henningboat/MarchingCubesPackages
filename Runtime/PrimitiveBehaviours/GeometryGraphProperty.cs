﻿using System;
using henningboat.CubeMarching;
using henningboat.CubeMarching.Utils.Containers;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    [Serializable]
    public sealed class GeometryGraphProperty
    {
        public int Index;
        public SerializableGUID ID;
        public readonly string DebugInformation;
        public readonly GeometryPropertyType Type;
        public float32 DefaultValue;

        public bool IsExposed => ID != default;
        
        public int GetSizeInBuffer()
        {
            return Type switch
            {
                GeometryPropertyType.Float => 1,
                GeometryPropertyType.Float3 => 3,
                GeometryPropertyType.Color32 => 1,
                GeometryPropertyType.Float4X4 => 16,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public GeometryGraphProperty(int index, SerializableGUID id, GeometryPropertyType type, float32 value, string name, string debugInformation)
        {
            ID = id;
            Index = index;
            Type = type;
            DebugInformation = debugInformation;
            DefaultValue = value;
            Name = name;
        }

        public string Name;

        public override string ToString()
        {
            return DebugInformation;
        }
    }
}