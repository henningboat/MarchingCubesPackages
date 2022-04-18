using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.Rendering;
using henningboat.CubeMarching.Runtime.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    [BurstCompile]
    public struct JCalculateTriangulationIndicesJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<CTriangulationInstruction> TriangulationInstructions;

        [NativeDisableParallelForRestriction]
        public NativeArray<CSubChunkWithTrianglesIndex> SubChunksWithTrianglesData;

        [NativeDisableParallelForRestriction] public NativeArray<int> VertexCountPerSubChunk;
        [NativeDisableParallelForRestriction] public GeometryFieldData GeometryField;

        public void Execute(int clusterIndex)
        {
            var cluster = GeometryField.GetCluster(clusterIndex);
            var offsetInSubChunkBuffer = clusterIndex * Constants.subChunksPerCluster;

            var clusterParameters = cluster.Parameters;
            clusterParameters.needsIndexBufferUpdate = false;

            var subChunksWithTrianglesCount = 0;
            var subChunksWithTrianglesSlice =
                SubChunksWithTrianglesData.Slice(clusterIndex * Constants.subChunksPerCluster,
                    Constants.subChunksPerCluster);

            var totalVertexCount = 0;

            var vertexCountPerSubChunk =
                VertexCountPerSubChunk.Slice(offsetInSubChunkBuffer, Constants.subChunksPerCluster);

            var triangulationInstructions =
                TriangulationInstructions.SliceList(offsetInSubChunkBuffer, Constants.subChunksPerCluster);

            for (var chunkIndex = 0; chunkIndex < Constants.chunksPerCluster; chunkIndex++)
            {
                var chunkParameters = cluster.GetChunk(chunkIndex).Parameters;
                var positionOfChunkWS = chunkParameters.PositionWS;

                // var currentHash = dynamicData.DistanceFieldChunkData.CurrentGeometryInstructionsHash;

                if (!cluster.Parameters.WriteMask[chunkIndex] && false) continue;

                for (var i = 0; i < 8; i++)
                {
                    var subChunkIndex = chunkIndex * 8 + i;

                    if (chunkParameters.InnerDataMask.GetBit(i) || true)
                    {
                        var subChunkOffset = Runtime.DistanceFieldGeneration.Utils.IndexToPositionWS(i, 2) * 4;


                        const int maxTrianglesPerSubChunk = 4 * 4 * 4 * 5;

                        //todo re-add checking for this 
                        if (GeometryChunkParameters.InstructionsChangedSinceLastFrame)
                        {
                            triangulationInstructions.Add(
                                new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, subChunkIndex));
                            vertexCountPerSubChunk[subChunkIndex] = maxTrianglesPerSubChunk;
                        }

                        subChunksWithTrianglesSlice[subChunksWithTrianglesCount] = new CSubChunkWithTrianglesIndex
                        {
                            SubChunkIndex = subChunkIndex, ChunkPositionGS = positionOfChunkWS + subChunkOffset
                        };
                        subChunksWithTrianglesCount++;

                        totalVertexCount += vertexCountPerSubChunk[subChunkIndex];
                    }
                    else
                    {
                        vertexCountPerSubChunk[subChunkIndex] = 0;
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