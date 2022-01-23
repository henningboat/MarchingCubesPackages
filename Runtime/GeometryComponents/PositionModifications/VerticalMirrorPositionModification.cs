using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct VerticalMirrorPositionModification : IPositionModification
    {
        public PositionModificationType Type => PositionModificationType.VerticalMirror;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
        {
            positionWS.y = SimdMath.abs(positionWS.y);
            return positionWS;
        }
    }
}