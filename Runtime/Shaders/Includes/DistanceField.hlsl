#ifndef DISTANCE_FIELD_INCLUDED
#define DISTANCE_FIELD_INCLUDED

#include "Constants.hlsl"
#include "MortonNumbers.hlsl"

struct MaterialData4{
    int4 data;
};

struct PackedDistanceFieldData4{
    float4 surfaceDistance;
    MaterialData4 terrainMaterial;
};

int indexFromCoordAndGridSize(int3 position, int3 gridSize)
{
    return position.z * gridSize.y * gridSize.x + position.y * gridSize.x + position.x;
}

uint _ChunkCountsX;
uint _ChunkCountsY;
uint _ChunkCountsZ;

uint3 GetChunkCounts()
{
    return uint3(_ChunkCountsX,_ChunkCountsY,_ChunkCountsZ);
}

StructuredBuffer<int> _IndexMap;
StructuredBuffer<PackedDistanceFieldData4> _DistanceField;

uint GetChunkIndexFromPosition(float3 position)
{
   const uint3 chunkPosition = clamp(position/k_ChunkLength,  0, GetChunkCounts());
    return indexFromCoordAndGridSize(chunkPosition,GetChunkCounts());
}

uint DistanceFieldIndexFromPositionWS(uint3 position)
{
    const uint chunkIndex = GetChunkIndexFromPosition(position);
    const uint baseIndexInDistanceFieldBuffer = _IndexMap[chunkIndex];
    
    return baseIndexInDistanceFieldBuffer + EncodeMorton3(position%k_ChunkLength);
}

#endif