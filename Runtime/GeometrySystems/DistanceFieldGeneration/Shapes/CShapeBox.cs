using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeBox : ITerrainModifierShape
    {
        [FieldOffset(0)] public Float3Value Extends;
        
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            var extends = Extends.Resolve(valueBuffer);
            PackedFloat3 q = abs(positionWS) - extends;
            return length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
        }

        public ShapeType Type => ShapeType.Box;

    }
}