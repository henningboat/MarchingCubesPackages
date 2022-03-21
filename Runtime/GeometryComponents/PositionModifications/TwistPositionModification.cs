using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct TwistPositionModification : IPositionModification
    {
        [FieldOffset(0)] [DefaultValue(20)] public float twistAmount;

        public PositionModificationType Type => PositionModificationType.Twist;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            var c = SimdMath.cos(twistAmount * positionWS.y);
            var s = SimdMath.sin(twistAmount * positionWS.y);

            return new PackedFloat3(c * positionWS.x - s * positionWS.z, 
                positionWS.y, positionWS.x * s + positionWS.z * c);
        }
    }
}