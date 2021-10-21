using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct CTerrainTransformationRepetition : ITerrainTransformation
    {
        [FieldOffset(0)] public Float3Value Period;

        public TerrainTransformationType TerrainTransformationType => TerrainTransformationType.Repetition;

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            var period = Period.Resolve(valueBuffer);
            return positionWS % period;
        }
    }
}