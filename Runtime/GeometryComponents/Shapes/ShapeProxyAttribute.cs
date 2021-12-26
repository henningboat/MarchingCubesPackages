using System;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    public class ShapeProxyAttribute : Attribute
    {
        public readonly ShapeType ShapeType;

        public ShapeProxyAttribute(ShapeType shapeType)
        {
            ShapeType = shapeType;
        }
    }
}