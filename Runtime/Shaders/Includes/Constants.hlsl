#ifndef CONSTANTS_INCLUDED
#define CONSTANTS_INCLUDED

static const int k_ChunkLength = 8;
static const int k_ChunkVolume =  k_ChunkLength*k_ChunkLength*k_ChunkLength;
static const int k_PackedCapacity =4;
static const int k_MaxTrianglesPerChunk = k_ChunkVolume*5;

#endif