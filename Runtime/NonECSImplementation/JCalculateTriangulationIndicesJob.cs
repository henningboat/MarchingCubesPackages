using System;
using Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils;

namespace NonECSImplementation
{
    [BurstCompile]
    public struct JCalculateTriangulationIndicesJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<CTriangulationInstruction> TriangulationInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<CSubChunkWithTrianglesIndex> SubChunksWithTrianglesData;
        [NativeDisableParallelForRestriction] public NativeArray<CVertexCountPerSubCluster> VertexCountPerSubChunk;

        public NativeArray<CClusterParameters> ClusterParameters;
            
        public void Execute(int clusterIndex)
        {
            int offsetInSubChunkBuffer = clusterIndex * Constants.subChunksPerCluster;

            var clusterParameters = ClusterParameters[clusterIndex];
            clusterParameters.needsIndexBufferUpdate = false;

            // NativeSlice<int> vertexCountData = default;
            //     var hasVertexCountReadback = false;
            //     var vertexCountReadbackTimesStamp = 0;
    
            //todo reimplement GPUReadbacks
            // for (var i = 0; i < gpuReadbackDataClusterIndex.Length; i++)
            // {
            //     if (gpuReadbackDataClusterIndex[i] == clusterPosition.ClusterIndex)
            //     {
            //         vertexCountData = new NativeSlice<int>(gpuReadbackDataVertexCount, Constants.SubChunksInCluster * i, Constants.SubChunksInCluster);
            //         vertexCountReadbackTimesStamp = gpuReadbackDataFrameTimestamp[i];
            //         hasVertexCountReadback = true;
            //         break;
            //     }
            // }
    
            var totalVertexCount = 0;

             NativeSlice<CVertexCountPerSubCluster> vertexCountPerSubChunk = VertexCountPerSubChunk.Slice(offsetInSubChunkBuffer, Constants.subChunksPerCluster);
            // NativeSlice<CSubChunkWithTrianglesIndex> subChunksWithTriangles = SubChunksWithTrianglesData.Slice(offsetInSubChunkBuffer, GeometryFieldData.subChunksPerCluster);
            //

            var triangulationInstructions = TriangulationInstructions.SliceList(offsetInSubChunkBuffer, Constants.subChunksPerCluster);

            for (var chunkIndex = 0; chunkIndex < Constants.chunksPerCluster; chunkIndex++)
            {
                var positionOfChunkWS = TerrainChunkEntitySystem.Utils.IndexToPositionWS(chunkIndex, new int3(Constants.chunkLengthPerCluster))*Constants.chunkLength;

                //toto read actual inner data masks ones those exist
                byte innerDataMask = Byte.MaxValue;
                //
                // var currentHash = dynamicData.DistanceFieldChunkData.CurrentGeometryInstructionsHash;

                for (var i = 0; i < 8; i++)
                {
                    var subChunkIndex = chunkIndex * 8 + i;

                    // if (hasVertexCountReadback)
                    // {
                    //     if (dynamicData.DistanceFieldChunkData.InstructionChangeFrameCount <= vertexCountReadbackTimesStamp)
                    //     {
                    //         vertexCountPerSubChunk[subChunkIndex] = new CVertexCountPerSubCluster() {vertexCount = vertexCountData[subChunkIndex]};
                    //         clusterParameters.needsIndexBufferUpdate = true;
                    //     }
                    // }

                    if (innerDataMask.GetBit(i))
                    {
                        var subChunkOffset = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 2) * 4;
                        
//                        triangulationInstructions.Add( new CSubChunkWithTrianglesIndex(positionOfChunkWS + subChunkOffset, 0, true));

                        //todo re-add checking for this 
                        //   if (dynamicData.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
                        {
                            triangulationInstructions.Add( new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, subChunkIndex));
                            vertexCountPerSubChunk[subChunkIndex] = new CVertexCountPerSubCluster() {vertexCount = Rendering.Constants.maxVertsPerCluster};
                            SubChunksWithTrianglesData[subChunkIndex] = new CSubChunkWithTrianglesIndex()
                            {
                                SubChunkIndex = subChunkIndex, ChunkPositionGS = positionOfChunkWS + subChunkOffset
                            };
                        }

                        totalVertexCount += vertexCountPerSubChunk[subChunkIndex].vertexCount;
                    }
                }
            }

            clusterParameters.vertexCount = totalVertexCount;
            clusterParameters.triangulationInstructionCount = triangulationInstructions.Count;

            ClusterParameters[clusterIndex] = clusterParameters;
        }
    }
}