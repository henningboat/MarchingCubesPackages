using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using static Code.SIMDMath.SimdMath;


namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct CShapeSphere : ITerrainModifierShape
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

        [FieldOffset(0)] public float radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            var radiusValue = radius;
            return length(positionOS) - radiusValue;
        }

        public ShapeType Type => ShapeType.Sphere;
    }
}