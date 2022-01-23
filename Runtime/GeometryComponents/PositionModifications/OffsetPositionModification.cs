using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct OffsetPositionModification : IPositionModification
    {
        [FieldOffset(0)] [DefaultValue(0, 0, 0)]
        public float3 Offset;
        
        public PositionModificationType Type => PositionModificationType.Offset;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            return positionWS - Offset;
        }
    }
}