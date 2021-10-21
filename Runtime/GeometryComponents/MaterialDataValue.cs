using System;
using henningboat.CubeMarching.TerrainChunkSystem;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometryComponents
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