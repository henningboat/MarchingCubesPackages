#include "MarchTables.compute"
 int3 _DistanceFieldExtends;

int3 IndexToPositionWS(int i, int3 size)
{
    int index = i;

    int x = index % size.x;
    int y = index / size.x % size.y;
    int z = index / (size.x * size.y);

    return int3(x, y, z);
}

int PositionToIndex(int3 position, int3 size)
{
    return position.x + position.y * size.x + position.z * size.x * size.y;
}

int PackVertexData(int3 positionWS, int edgeA, int edgeB)
{
    _DistanceFieldExtends = 64;
    
    int positionIndex = PositionToIndex(positionWS,_DistanceFieldExtends);
    positionIndex*=64;
    positionIndex+=edgeA*8;
    positionIndex+=edgeB;

    return positionIndex;
}

void UnpackVertexData(int packedVertex, out int3 positionWS, out int edgeA, out int edgeB)
{
    _DistanceFieldExtends = 64;
    
    positionWS = IndexToPositionWS(packedVertex/64,_DistanceFieldExtends);
    edgeA = packedVertex%64/8;
    edgeB = packedVertex % 8;
}

int numPointsPerAxis;


uint3 _TerrainMapSize;
int _MaterialIDFilter;

struct PackedTerrainMaterial{
    int4 data;
};

struct TerrainData4{
    float4 surfaceDistance;
    PackedTerrainMaterial terrainMaterial;
};

StructuredBuffer<TerrainData4> _GlobalTerrainBuffer;
StructuredBuffer<int> _GlobalTerrainIndexMap;

static const int terrainChunkLength = 8;


float4 unpackVertexColor(uint i)
{
    float4 unpack;
    
    unpack.x = float((i & uint(0xff000000)) >> 24);
    unpack.y = float((i & uint(0x00ff0000)) >> 16);
    unpack.z = float((i & uint(0x0000ff00)) >> 8);
    unpack.w = float((i & uint(0x000000ff)) >> 0);
    
    return unpack;
}



float3 interpolateVerts(float4 v1, float4 v2, out float t) {
    t = (0 - v1.w) / (v2.w - v1.w);
    return v1.xyz + t * (v2.xyz-v1.xyz);
}

float3 interpolateNormals(float3 v1, float3 v2, float t) {
    return v1.xyz + t * (v2.xyz-v1.xyz);
}


float4 interpolateColors(float4 v1, float4 v2, float t) {
    return v1 + t * (v2-v1);
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

 int GetIndexOfVert(float3 positionOS, int3 basePositionOfChunk)
{
    const int3 positionIndex = floor(positionOS);
    const int index = positionIndex.x+positionIndex.y*8+positionIndex.z*64;
    return index;
}

 float3 CalculateNormalForPosition(int3 localPosition)
{
    float3 combinedNormal=0.00001;
    for (int x=-1;x<2;x++)
        for (int y=-1;y<2;y++)
            for (int z=-1;z<2;z++)
            {
                if(!(x==0&&y==0&&z==0))
                {
                    //const float surfaceDistance = _grid[GridPositionToIndex(int3(localPosition.x+x, localPosition.y+y, localPosition.z+z))].w;
                    const float surfaceDistance = GetPointPosition(int3(localPosition.x+x, localPosition.y+y, localPosition.z+z)).w;
           
                    combinedNormal += float3(x,y,z)*surfaceDistance;
                }
            }

    return normalize(combinedNormal);
}


int GetCubeMaterialData(uint3 position)
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
    

    return _GlobalTerrainBuffer[indexInTerrainBuffer / 4].terrainMaterial.data[indexInTerrainBuffer % 4];
}

int3 _ClusterPositionWS;

float3 interpolateMaterial(uint a, uint b, float t)
{
    //return t<0.5?a:b;
    float4 aUnpacked = unpackVertexColor(a);
    float4 bUnpacked = unpackVertexColor(b);
    
    //todo check if saturate is actually needed here
    
    float4 blended = lerp(aUnpacked, bUnpacked, saturate(t));

    return blended.rgb;
}

 void GetVertexDataFromPackedVertex(int packedVertex, out float3 vertexPosition, out float3 normal, out float3 color)
{
    int3 positionWS;
    int a0;
    int b0;
     
    UnpackVertexData(packedVertex, positionWS,a0,b0);
     
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
    

    float tA;
     
    vertexPosition = interpolateVerts(cubeCorners[a0], cubeCorners[b0], tA)-_ClusterPositionWS;
    normal = interpolateNormals(cubeNormals[a0], cubeNormals[b0],tA);

    color=interpolateMaterial(cubeVertexColors[a0], cubeVertexColors[b0],tA);


    vertexPosition=GetPointPosition(positionWS).xyz;
    vertexPosition = lerp(cubeCorners[a0], cubeCorners[b0], 0.5);
}