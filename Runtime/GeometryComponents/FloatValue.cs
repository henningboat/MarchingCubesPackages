using System;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
{
    [Serializable]
    public struct FloatValue
    {
        public int Index;

        public float Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer[Index];
        }
    }
}