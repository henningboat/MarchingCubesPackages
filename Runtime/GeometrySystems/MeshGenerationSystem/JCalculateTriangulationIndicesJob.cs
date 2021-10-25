using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Rendering;
using henningboat.CubeMarching.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    [BurstCompile]
    public struct JCalculateTriangulationIndicesJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<CTriangulationInstruction> TriangulationInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<CSubChunkWithTrianglesIndex> SubChunksWithTrianglesData;
        [NativeDisableParallelForRestriction] public NativeArray<CVertexCountPerSubCluster> VertexCountPerSubChunk;
        [NativeDisableParallelForRestriction] public GeometryFieldData GeometryField;

        public void Execute(int clusterIndex)
        {
            var cluster = GeometryField.GetCluster(clusterIndex);
            int offsetInSubChunkBuffer = clusterIndex * Constants.subChunksPerCluster;

            var clusterParameters = cluster.Parameters;
            clusterParameters.needsIndexBufferUpdate = false;

            int subChunksWithTrianglesCount=0;

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
                var chunkParameters = cluster.GetChunk(chunkIndex).Parameters;
                var positionOfChunkWS = chunkParameters.PositionWS;

                // var currentHash = dynamicData.DistanceFieldChunkData.CurrentGeometryInstructionsHash;

                if (!cluster.Parameters.WriteMask[chunkIndex]) continue;

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

                    if (chunkParameters.InnerDataMask.GetBit(i))
                    {
                        var subChunkOffset = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 2) * 4;

                        //todo re-add checking for this 
                        if (chunkParameters.InstructionsChangedSinceLastFrame)
                        {
                            triangulationInstructions.Add( new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, subChunkIndex));
                            vertexCountPerSubChunk[subChunkIndex] = new CVertexCountPerSubCluster() {vertexCount = Constants.maxVertsPerCluster};
                        }

                        SubChunksWithTrianglesData[subChunksWithTrianglesCount] = new CSubChunkWithTrianglesIndex
                        {
                            SubChunkIndex = subChunkIndex, ChunkPositionGS = positionOfChunkWS + subChunkOffset
                        };
                        subChunksWithTrianglesCount++;
                           
                        totalVertexCount += vertexCountPerSubChunk[subChunkIndex].vertexCount;
                    }
                }
            }

            clusterParameters.vertexCount = totalVertexCount;
            clusterParameters.triangulationInstructionCount = triangulationInstructions.Count;
            clusterParameters.subChunksWithTrianglesCount = subChunksWithTrianglesCount;

            cluster.Parameters = clusterParameters;
        }
    }
}