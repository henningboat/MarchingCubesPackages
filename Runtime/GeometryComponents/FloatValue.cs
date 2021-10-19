using System;
using Unity.Collections;

namespace GeometryComponents
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