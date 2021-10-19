using System;
using TerrainChunkSystem;
using Unity.Collections;

namespace GeometryComponents
{
    [Serializable]
    public struct MaterialDataValue
    {
        public int Index;

        public TerrainMaterial Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer.Reinterpret<TerrainMaterial>()[Index];
        }
    }
}