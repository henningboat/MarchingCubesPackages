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
        [NativeDisableParallelForRestriction] public NativeArray<int> VertexCountPerSubChunk;
        [NativeDisableParallelForRestriction] public GeometryFieldData GeometryField;
        
        public void Execute(int clusterIndex)
        {
            var cluster = GeometryField.GetCluster(clusterIndex);
            int offsetInSubChunkBuffer = clusterIndex * Constants.subChunksPerCluster;

            var clusterParameters = cluster.Parameters;
            clusterParameters.needsIndexBufferUpdate = false;

            int subChunksWithTrianglesCount = 0;
            var subChunksWithTrianglesSlice =
                SubChunksWithTrianglesData.Slice(clusterIndex * Constants.subChunksPerCluster,
                    Constants.subChunksPerCluster);

            var totalVertexCount = 0;

            NativeSlice<int> vertexCountPerSubChunk = VertexCountPerSubChunk.Slice(offsetInSubChunkBuffer, Constants.subChunksPerCluster);

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

                    if (chunkParameters.InnerDataMask.GetBit(i))
                    {
                        var subChunkOffset = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 2) * 4;

                        
                        const int maxTrianglesPerSubChunk = 4 * 4 * 4 * 5;
                        
                        //todo re-add checking for this 
                        if (chunkParameters.InstructionsChangedSinceLastFrame)
                        {
                            triangulationInstructions.Add( new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, subChunkIndex));
                            vertexCountPerSubChunk[subChunkIndex] = maxTrianglesPerSubChunk;
                        }

                        subChunksWithTrianglesSlice[subChunksWithTrianglesCount] = new CSubChunkWithTrianglesIndex
                        {
                            SubChunkIndex = subChunkIndex, ChunkPositionGS = positionOfChunkWS + subChunkOffset
                        };
                        subChunksWithTrianglesCount++;
                           
                        totalVertexCount += vertexCountPerSubChunk[subChunkIndex];
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