using System;
using TerrainChunkSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Authoring
{
    [Serializable]
    public struct TerrainChunkDataBuffer : IBufferElementData
    {
        public TerrainChunkData Value;
    }

    [Serializable]
    public struct TerrainChunkIndexMap : IBufferElementData
    {
        public int Index;
    }

    public struct TotalClusterCounts : IComponentData
    {
        public int3 Value;
    }
}