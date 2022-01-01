using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using SIMDMath;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications
{
    [Serializable]
    public struct OnionDistanceModification : IDistanceModification
    {
        [SerializeField] [DefaultValue(2)] private float _thickness;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance)
        {
            return SimdMath.abs(currentDistance) - _thickness;
        }

        public DistanceModificationType Type => DistanceModificationType.Onion;
    }
}