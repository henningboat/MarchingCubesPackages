using System;
using Unity.Collections;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometryComponents
{
    [Serializable]
    public struct Float3Value
    {
        public int Index;

        public float3 Resolve(NativeArray<float> valueBuffer)
        {
            return new(valueBuffer[Index], valueBuffer[Index + 1], valueBuffer[Index + 2]);
        }
    }
}