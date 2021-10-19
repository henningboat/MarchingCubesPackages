using System;
using TerrainChunkEntitySystem;

using Unity.Mathematics;

namespace GeometryComponents
{
    [Serializable]
    public struct CGeometryCombiner
    {
        #region Public Fields

        public FloatValue BlendFactor;
        public CombinerOperation Operation;

        #endregion

        public uint CalculateHash()
        {
            var hash = math.asuint(BlendFactor.Index);
            hash.AddToHash((uint) Operation);
            return hash;
        }
    }
}