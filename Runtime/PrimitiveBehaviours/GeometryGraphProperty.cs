using System;
using henningboat.CubeMarching;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public abstract class GeometryGraphProperty
    {
        public int Index;
        public readonly string DebugInformation;
        public readonly GeometryPropertyType Type;
        protected object Value { get; set; } = default;

        public T GetValue<T>()
        {
            return Value switch
            {
                null => default,
                T tValue => tValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int GetSizeInBuffer()
        {
            return Type switch
            {
                GeometryPropertyType.Float => 1,
                GeometryPropertyType.Float3 => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected GeometryGraphProperty(int index, GeometryPropertyType type, string debugInformation)
        {
            Index = index;
            Type = type;
            DebugInformation = debugInformation;
        }

        public override string ToString()
        {
            return DebugInformation;
        }
    }
}