using System;
using Unity.Collections;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometryComponents
{
    [Serializable]
    public struct Float4X4Value
    {
        public int Index;

        public float4x4 Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer.Read<float4x4>(Index);
        }
    }
}