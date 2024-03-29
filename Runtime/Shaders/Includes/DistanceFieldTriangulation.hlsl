﻿#include "MarchTables.compute"
#include "MortonNumbers.hlsl"

int3 IndexToPositionWS(int i, int3 size)
{
    int index = i;

    int x = index % size.x;
    int y = index / size.x % size.y;
    int z = index / (size.x * size.y);

    return int3(x, y, z);
}

int _TerrainMapSizeX;
int _TerrainMapSizeY;
int _TerrainMapSizeZ;

int _FieldVoxelSizeX;
int _FieldVoxelSizeY;
int _FieldVoxelSizeZ;

int PositionToIndex(int3 position, int3 size)
{
    return position.x + position.y * size.x + position.z * size.x * size.y;
}

//Cluster Triangle
struct ClusterTriangle
{
    uint value;

    #define PositionPartBitmask 0x3FFFF;

    int3 GetPositionInCluster()
    {
        int positionIndex = value & PositionPartBitmask;
        return IndexToPositionWS(positionIndex, 64);
    }

    int GetCubeIndex()
    {
        int cubeIndex = value >> 18;
        return cubeIndex;
    }
};

ClusterTriangle PackTriangle(int3 positionInCluster, int cubeIndex, int offsetInsideCube)
{
    int3 cluserSize = 64;
    uint positionIndex = PositionToIndex(positionInCluster, cluserSize);

    uint combinedCubeIndex = cubeIndex * 5 + offsetInsideCube;

    ClusterTriangle clusterTriangle;
    clusterTriangle.value = positionIndex;
    clusterTriangle.value |= combinedCubeIndex << 18;
    return clusterTriangle;
}

//--Cluster Triangle

int numPointsPerAxis;


int _MaterialIDFilter;

struct PackedTerrainMaterial
{
    int4 data;
};

struct TerrainData4
{
    float4 surfaceDistance;
    PackedTerrainMaterial terrainMaterial;
};

StructuredBuffer<TerrainData4> _GlobalTerrainBuffer;
StructuredBuffer<int> _GlobalTerrainIndexMap;

static const int terrainChunkLength = 8;


float4 unpackVertexColor(uint i)
{
    float4 unpack;

    unpack.w = float((i & uint(0xff000000)) >> 24);
    unpack.z = float((i & uint(0x00ff0000)) >> 16);
    unpack.y = float((i & uint(0x0000ff00)) >> 8);
    unpack.x = float((i & uint(0x000000ff)) >> 0);

    return unpack;
}


float3 interpolateVerts(float4 v1, float4 v2, out float t)
{
    t = (0 - v1.w) / (v2.w - v1.w);

    //todo should not be neccecary
    t = saturate(t);
    
    return v1.xyz + t * (v2.xyz - v1.xyz);
}

float3 interpolateNormals(float3 v1, float3 v2, float t)
{
    return v1.xyz + t * (v2.xyz - v1.xyz);
}


float4 interpolateColors(float4 v1, float4 v2, float t)
{
    return v1 + t * (v2 - v1);
}

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

    const uint indexInTerrainIndexMap = indexFromCoordAndGridSize(positionInIndexMap,
                                                                  int3(_TerrainMapSizeX, _TerrainMapSizeY,
                                                                       _TerrainMapSizeZ));
    return indexInTerrainIndexMap;
}


float4 GetPointPosition(uint3 position)
{
    const uint indexInTerrainBuffer = EncodeMorton3(position);

    float surfaceDistance = _GlobalTerrainBuffer[indexInTerrainBuffer / 4].surfaceDistance[indexInTerrainBuffer % 4];

    if(any(position<1||position>int3(_FieldVoxelSizeX,_FieldVoxelSizeY,_FieldVoxelSizeZ)-1))
    {
        surfaceDistance=0.1;
    }
    
    return float4(position.x, position.y, position.z, surfaceDistance);
}

int GetIndexOfVert(float3 positionOS, int3 basePositionOfChunk)
{
    const int3 positionIndex = floor(positionOS);
    const int index = positionIndex.x + positionIndex.y * 8 + positionIndex.z * 64;
    return index;
}

float3 CalculateNormalForPosition(int3 localPosition)
{
    float3 combinedNormal = 0.00001;
    for (int x = -1; x < 2; x++)
        for (int y = -1; y < 2; y++)
            for (int z = -1; z < 2; z++)
            {
                if (!(x == 0 && y == 0 && z == 0))
                {
                    //const float surfaceDistance = _grid[GridPositionToIndex(int3(localPosition.x+x, localPosition.y+y, localPosition.z+z))].w;
                    const float surfaceDistance = GetPointPosition(
                        int3(localPosition.x + x, localPosition.y + y, localPosition.z + z)).w;

                    combinedNormal += float3(x, y, z) * surfaceDistance;
                }
            }

    return (combinedNormal);
}


int GetCubeMaterialData(uint3 position)
{
    const uint positionInIndexMap = GetPointPositionInIndexMap(position);
    const uint terrainChunkCapacity = 512;
    int chunkIndex = _GlobalTerrainIndexMap[positionInIndexMap];
    // chunkIndex=2+8;
    const uint baseIndexOfTerrainChunk = chunkIndex * terrainChunkCapacity;

    const uint3 positionWithinTerrainChunk = position % 8;

    const int subChunkIndex = indexFromCoordAndGridSize(positionWithinTerrainChunk / 4, 2);

    const uint indexWithinSubChunk = indexFromCoordAndGridSize(position % 4, 4);
    const uint indexInTerrainBuffer = baseIndexOfTerrainChunk + subChunkIndex * 64 + indexWithinSubChunk;


    return _GlobalTerrainBuffer[indexInTerrainBuffer / 4].terrainMaterial.data[indexInTerrainBuffer % 4];
}

int3 _ClusterPositionWS;

float3 interpolateMaterial(uint a, uint b, float t)
{
    //return t<0.5?a:b;
    float4 aUnpacked = unpackVertexColor(a);
    float4 bUnpacked = unpackVertexColor(b);

    //todo check if saturate is actually needed here

    float4 blended = lerp(aUnpacked, bUnpacked, saturate(t))/255;

    return blended.rgb;
}

int _PositionInClusterX;
int _PositionInClusterY;
int _PositionInClusterZ;

float DoRay(float3 positionWS, float3 normalWS)
{
    positionWS+=normalWS*2;
    const int iterations = 10;
    for(int i=1;i<iterations;i++)
    {
        float distance = GetPointPosition(round(positionWS)).w;
        if(distance<0)
        {
            return i/iterations;
        }
        positionWS+=normalWS*i;
    }
    return 1;
}


float random (float2 st) {
   // st+=_Time.x;
    return frac(sin(dot(st.xy,
                         float2(12.9898,78.233)))*
        43758.5453123);
}


float3 RandomDirection(int i, int3 positionWs, float3 direction)
{
    float f = i*i*3.59230+2389.4324;
    float g = positionWs.x+positionWs.y*3.432+positionWs.z*8.34234;
    return (float3(random(float2(g,f)),random(float2(g,f+3)),random(float2(float2(g,f+5))))-0.5f);
   // return (float3(random(float2(positionWs.xy+f)),random(positionWs.yz*f),random(float2(positionWs.zx*f)))-0.5f).x;
}

float3 RayMarchAO(float3 normalWS, int3 positionWS)
{
    float3 directionSum = 0;
    float ao=0;
    const uint count = 10;
    for (int i=0;i<count;i++){
        const float3 direction = normalize(normalize(normalWS)+0.7*normalize(RandomDirection(i, positionWS, normalWS)));
        directionSum+=direction;
        ao += DoRay(positionWS, direction);
    }
    //return normalize(directionSum)/count;
    return ao/count;
}

void GetVertexDataFromPackedVertex(ClusterTriangle clusterTriangle, int vertexIndexInCluster, out float3 vertexPosition,
                                   out float3 normal,
                                   out float3 color,
                                   out float occlusion)
{
    int triangleTypeIndex;

    int3 positionInCluster = clusterTriangle.GetPositionInCluster();

    int3 positionWS = positionInCluster + int3(_PositionInClusterX,_PositionInClusterY,_PositionInClusterZ);
    
    triangleTypeIndex = 16;

    float4 cubeCorners[8] = {
        GetPointPosition((int3(positionWS.x, positionWS.y, positionWS.z))),
        GetPointPosition((int3(positionWS.x + 1, positionWS.y, positionWS.z))),
        GetPointPosition((int3(positionWS.x + 1, positionWS.y, positionWS.z + 1))),
        GetPointPosition((int3(positionWS.x, positionWS.y, positionWS.z + 1))),
        GetPointPosition((int3(positionWS.x, positionWS.y + 1, positionWS.z))),
        GetPointPosition((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z))),
        GetPointPosition((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1))),
        GetPointPosition((int3(positionWS.x, positionWS.y + 1, positionWS.z + 1)))
    };

    int cubeVertexColors[8] = {
        GetCubeMaterialData((int3(positionWS.x, positionWS.y, positionWS.z))),
        GetCubeMaterialData((int3(positionWS.x + 1, positionWS.y, positionWS.z))),
        GetCubeMaterialData((int3(positionWS.x + 1, positionWS.y, positionWS.z + 1))),
        GetCubeMaterialData((int3(positionWS.x, positionWS.y, positionWS.z + 1))),
        GetCubeMaterialData((int3(positionWS.x, positionWS.y + 1, positionWS.z))),
        GetCubeMaterialData((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z))),
        GetCubeMaterialData((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1))),
        GetCubeMaterialData((int3(positionWS.x, positionWS.y + 1, positionWS.z + 1)))
    };

    float3 cubeNormals[8] = {
        CalculateNormalForPosition((int3(positionWS.x, positionWS.y, positionWS.z))),
        CalculateNormalForPosition((int3(positionWS.x + 1, positionWS.y, positionWS.z))),
        CalculateNormalForPosition((int3(positionWS.x + 1, positionWS.y, positionWS.z + 1))),
        CalculateNormalForPosition((int3(positionWS.x, positionWS.y, positionWS.z + 1))),
        CalculateNormalForPosition((int3(positionWS.x, positionWS.y + 1, positionWS.z))),
        CalculateNormalForPosition((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z))),
        CalculateNormalForPosition((int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1))),
        CalculateNormalForPosition((int3(positionWS.x, positionWS.y + 1, positionWS.z + 1)))
    };

    const float3 aoCube[8] = {
        RayMarchAO(cubeNormals[0],(int3(positionWS.x, positionWS.y, positionWS.z))),
        RayMarchAO(cubeNormals[1],(int3(positionWS.x + 1, positionWS.y, positionWS.z))),
        RayMarchAO(cubeNormals[2],(int3(positionWS.x + 1, positionWS.y, positionWS.z + 1))),
        RayMarchAO(cubeNormals[3],(int3(positionWS.x, positionWS.y, positionWS.z + 1))),
        RayMarchAO(cubeNormals[4],(int3(positionWS.x, positionWS.y + 1, positionWS.z))),
        RayMarchAO(cubeNormals[5],(int3(positionWS.x + 1, positionWS.y + 1, positionWS.z))),
        RayMarchAO(cubeNormals[6],(int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1))),
        RayMarchAO(cubeNormals[7],(int3(positionWS.x, positionWS.y + 1, positionWS.z + 1)))
    };

    float tA;


    int cubeIndex = clusterTriangle.GetCubeIndex();

    int indexIndex = triangulation[cubeIndex / 5][vertexIndexInCluster + 3 * (cubeIndex % 5)];

    int a0 = cornerIndexAFromEdge[indexIndex];
    int b0 = cornerIndexBFromEdge[indexIndex];

    vertexPosition = interpolateVerts(cubeCorners[a0], cubeCorners[b0], tA);
    normal = interpolateNormals(cubeNormals[a0], cubeNormals[b0], tA);

    color = interpolateMaterial(cubeVertexColors[a0], cubeVertexColors[b0], tA);
    occlusion = lerp(aoCube[a0].x, aoCube[b0].x, tA);
}
