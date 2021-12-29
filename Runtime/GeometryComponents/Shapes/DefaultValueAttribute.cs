using System;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    public class DefaultValueAttribute : Attribute
    {
        public readonly float[] DefaultValue;

        public DefaultValueAttribute(params float[] defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}