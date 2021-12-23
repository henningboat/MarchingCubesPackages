using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeBox : IGeometryShapeResolver
    {
        [FieldOffset(0)] public float3 Extends;
        
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            var extends = Extends;
            PackedFloat3 q = abs(positionWS) - extends;
            return length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
        }

        public ShapeType Type => ShapeType.Box;

    }
}