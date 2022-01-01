﻿using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct InversionDistanceModification : IDistanceModification
    {
        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return -currentDistance;
        }

        public DistanceModificationType Type => DistanceModificationType.Inversion;
    }

    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct InflationDistanceModification : IDistanceModification
    {
        [FieldOffset(0)] private float _inflationAmount;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return currentDistance + _inflationAmount;
        }

        public DistanceModificationType Type => DistanceModificationType.Inflation;
    }
}