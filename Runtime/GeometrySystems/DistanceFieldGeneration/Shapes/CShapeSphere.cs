using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;
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

        [FieldOffset(0)] public FloatValue radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            var radiusValue = radius.Resolve(valueBuffer);
            return length(positionOS) - radiusValue;
        }

        public ShapeType Type => ShapeType.Sphere;
    }
}