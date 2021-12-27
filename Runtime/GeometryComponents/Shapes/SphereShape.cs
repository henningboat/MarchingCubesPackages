using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct SphereShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8.0f)] public float radius;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            return length(positionOS) - radius;
        }

        public ShapeType ShapeType => ShapeType.Sphere;
    }
}