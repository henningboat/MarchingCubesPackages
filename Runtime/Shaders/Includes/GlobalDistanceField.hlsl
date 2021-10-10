﻿int numPointsPerAxis;


uint3 _TerrainMapSize;
int _MaterialIDFilter;


static const int terrainChunkLength = 8;


int indexFromCoordAndGridSize(int3 position, int3 gridSize)
{
    return position.z * gridSize.y * gridSize.x + position.y * gridSize.x + position.x;
}

int indexFromCoord(int x, int y, int z)
{
    return z * numPointsPerAxis * numPointsPerAxis + y * numPointsPerAxis + x;
}

int GetPointPositionInIndexMap(uint3 position)
{
    uint3 positionInIndexMap = 0;
    positionInIndexMap.x = position.x / 8;
    positionInIndexMap.y = position.y / 8;
    positionInIndexMap.z = position.z / 8;
    
    const uint indexInTerrainIndexMap = indexFromCoordAndGridSize(positionInIndexMap, _TerrainMapSize);
    return indexInTerrainIndexMap;
}

float4 GetPointPosition(uint3 position)
{
    const uint positionInIndexMap = GetPointPositionInIndexMap(position);
    const uint terrainChunkCapacity = 512;
     int chunkIndex = _GlobalTerrainIndexMap[positionInIndexMap];
   // chunkIndex=2+8;
    const uint baseIndexOfTerrainChunk = chunkIndex * terrainChunkCapacity;

    const uint3 positionWithinTerrainChunk = position % 8;

    const int subChunkIndex = indexFromCoordAndGridSize(positionWithinTerrainChunk/4,2);

    const uint indexWithinSubChunk = indexFromCoordAndGridSize(position % 4,4);
    const uint indexInTerrainBuffer = baseIndexOfTerrainChunk + subChunkIndex * 64 + indexWithinSubChunk;
    

    float surfaceDistance = _GlobalTerrainBuffer[indexInTerrainBuffer / 4].surfaceDistance[indexInTerrainBuffer % 4];

    if (position.x <= 0 || position.y <= 0 || position.z <= 0 ||
        position.x > _TerrainMapSize.x * 8 - 1 ||
        position.y > _TerrainMapSize.y * 8 - 1 ||
        position.z > _TerrainMapSize.z * 8 - 1)
    {
        surfaceDistance = 0.1f;
    }
    
    return float4(position.x, position.y, position.z, surfaceDistance);
}

float4 GetColorAtPosition(int3 position)
{
    const int positionInIndexMap = GetPointPositionInIndexMap(position);
    const int terrainChunkCapacity = 512;
    const int baseIndexOfTerrainChunk = _GlobalTerrainIndexMap[positionInIndexMap] * terrainChunkCapacity;

    const int3 positionWithinTerrainChunk = ((position + 8) % 8);
    const int indexWithinTerrainChunk = indexFromCoord(positionWithinTerrainChunk.x, positionWithinTerrainChunk.y, positionWithinTerrainChunk.z);

    const int indexInTerrainBuffer = baseIndexOfTerrainChunk + indexWithinTerrainChunk;

    float4 color = _GlobalTerrainBuffer[indexInTerrainBuffer / 4].terrainMaterial.data[indexInTerrainBuffer % 4] == 0 ? float4(1, 0, 0, 1) : float4(0, 1, 1, 1);

    return color;
}

float GetSurfaceDistanceInterpolated(float3 positionWS)
{
    int3 id = floor(positionWS);


    const float3 positinWithinCube = frac(positionWS);

    const float4 bottonPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y, id.z)), GetPointPosition(int3(id.x + 1, id.y, id.z)), positinWithinCube.x),
                                    lerp(GetPointPosition(int3(id.x, id.y, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    const float4 topPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y + 1, id.z)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z)), positinWithinCube.x),
                                 lerp(GetPointPosition(int3(id.x, id.y + 1, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    const float4 result = lerp(bottonPlane, topPlane, positinWithinCube.y);
    return result.w;
}

float rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 22.2830))) * 43758.5453);
}

// float RayMarchThickness(float3 positionWS, float3 viewDir)
// {
//     viewDir += rand(positionWS) * 0.1f;
//     float3 origin = positionWS;
//     positionWS += viewDir;
//
//
//     float maxDistance = 0;
//     float totalThickness = 0;
//
//     //first travel inside the object
//
//     float surfaceDistance = 0;
//     bool reachedOutside = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance > 0)
//         {
//             viewDir = 0;
//             reachedOutside = true;
//         }
//         else
//         {
//             totalThickness++;
//         }
//         viewDir += rand((positionWS) + i) * 0.01f;
//         positionWS += viewDir * 0.5;
//     }
//
//     if (reachedOutside == false)
//     {
//         return 1;
//     }
//
//     float3 sunDirection = float3(0, 1, 0);
//     positionWS += sunDirection;
//
//     bool hitSomething = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance < 0)
//         {
//             hitSomething = true;
//         }
//         positionWS += sunDirection;
//     }
//
//     if (hitSomething)
//     {
//         return 1;
//     }
//
//     //from the other side of the object
//     return 1;
//     return totalThickness / 30;
// }
