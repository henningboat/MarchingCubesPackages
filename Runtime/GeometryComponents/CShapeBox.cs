using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeBox : IComponentData, ITerrainModifierShape
    {
        [FieldOffset(0)] public Float3Value Extends;
        
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            var extends = Extends.Resolve(valueBuffer);
            PackedFloat3 q = abs(positionWS) - extends;
            return length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            return new() {min = int.MinValue, max = int.MaxValue};
        }

        public uint CalculateHash()
        {
            throw new NotImplementedException();
        }

        public ShapeType Type => ShapeType.Box;

    }
}