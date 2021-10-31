﻿using System.Runtime.InteropServices;
using Code.SIMDMath;

namespace henningboat.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Sequential,Size = 16)]
    public struct InversionDistanceModification : IDistanceModification
    {
        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return -currentDistance;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InflationDistanceModification : IDistanceModification
    {
        [FieldOffset(0)] private readonly float inflationAmount;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return currentDistance + inflationAmount;
        }
    }
}