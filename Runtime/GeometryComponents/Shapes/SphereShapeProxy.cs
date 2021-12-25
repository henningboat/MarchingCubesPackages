using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.PrimitiveBehaviours;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    [ShapeProxyAttribute(ShapeType.Sphere)]
    public class SphereShapeProxy : ShapeProxy
    {
        [PropertyTypeAttribute(GeometryPropertyType.Float)] private GeometryGraphProperty _radius;

        public SphereShapeProxy(GeometryGraphProperty radius, GeometryGraphProperty transformation) : base(transformation)
        {
            _radius = radius;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>()
            {
                _radius
            };
        }

        public override ShapeType ShapeType => ShapeType.Sphere;
    }

    public class ShapeProxyAttribute : Attribute
    {
        public readonly ShapeType ShapeType;

        public ShapeProxyAttribute(ShapeType shapeType)
        {
            ShapeType = shapeType;
        }
    }

    public class PropertyTypeAttribute : Attribute
    {
        public readonly GeometryPropertyType Type;

        public PropertyTypeAttribute(GeometryPropertyType type)
        {
            Type = type;
        }
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct SphereShapeResolver : IGeometryShapeResolver
    {
        #region ActualData

        [FieldOffset(0)] public float radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            var radiusValue = radius;
            return SimdMath.length(positionOS) - radiusValue;
        }
    }
}