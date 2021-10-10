using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Code.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapePlane : IComponentData, ITerrainModifierShape
    {
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            return positionWS.y;
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            return new() {min = int.MinValue, max = int.MaxValue};
        }

        public uint CalculateHash()
        {
            throw new NotImplementedException();
        }

        public ShapeType Type => ShapeType.Plane;

    }
}