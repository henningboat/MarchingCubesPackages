using System;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    public class PropertyTypeAttribute : Attribute
    {
        public readonly GeometryPropertyType Type;

        public PropertyTypeAttribute(GeometryPropertyType type)
        {
            Type = type;
        }
    }
}