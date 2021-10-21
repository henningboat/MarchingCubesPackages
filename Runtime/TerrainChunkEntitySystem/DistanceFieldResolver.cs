using System;
using System.Runtime.CompilerServices;
using Code.SIMDMath;
using GeometrySystems.GeometryFieldSetup;
using NonECSImplementation;
using TerrainChunkSystem;
using Unity.Collections;
using Unity.Mathematics;
using Utils;

namespace TerrainChunkEntitySystem
{
    public static class DistanceFieldResolver
    {
        public static void CalculateDistanceFieldForChunk(GeometryCluster cluster, GeometryChunk chunk, GeometryFieldData geometryField, GeometryGraphData geometryGraph)
        {
            var clusterParameters = cluster.Parameters;
            var chunkParameters = chunk.Parameters;
            
            //todo
            if (clusterParameters.WriteMask[chunkParameters.IndexInCluster]||true)
            {
                var positionsToCheck = new NativeArray<PackedFloat3>(2, Allocator.Temp);
                for (var i = 0; i < 2; i++)
                {
                    var offsetInChunk = new PackedFloat3(new float4(2, 6, 2, 6), new float4(2, 2, 6, 6), new float4(i) * 4 + 2);
                    positionsToCheck[i] = new PackedFloat3(chunkParameters.PositionWS) + offsetInChunk;
                }

                var iterator = new TerrainInstructionIterator(positionsToCheck, geometryGraph.GeometryInstructions, geometryGraph.ValueBuffer);

                iterator.CalculateTerrainData();

                GetCoverageAndFillMaskFromSurfaceDistance(iterator._terrainDataBuffer, out var mask, out var insideTerrainMask);

                chunkParameters.InnerDataMask = mask;
                chunkParameters.ChunkInsideTerrain = insideTerrainMask;

                positionsToCheck.Dispose();
            }
            else
            {
                chunkParameters.InnerDataMask = 0;
                return;
            }

            chunk.Parameters = chunkParameters;

            TerrainInstructionIterator detailIterator=default;
            var containsDetails = chunkParameters.InnerDataMask != 0;
            if (containsDetails)
            {
                CreatePositionsArray(chunk, out var positions);

                detailIterator = new TerrainInstructionIterator(positions, geometryGraph.GeometryInstructions, geometryGraph.ValueBuffer);
                detailIterator.CalculateTerrainData();
            }
            
            
            CopyResultsBackToBuffer(chunk, detailIterator);
            
            if (containsDetails)
            {
                detailIterator.Dispose();
            }

        }

        private static void CopyResultsBackToBuffer(GeometryChunk chunk, TerrainInstructionIterator iterator)
        {
            var readDataOffset = 0;
            for (var subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
            {
                if (chunk.Parameters.InnerDataMask.GetBit(subChunkIndex))
                {
                    for (var i = 0; i < 16; i++)
                    {
                        chunk[subChunkIndex * 16 + i] = iterator._terrainDataBuffer[readDataOffset + i];
                    }

                    readDataOffset += 16;
                }
                else
                {
                    for (var indexInSubChunk = 0; indexInSubChunk < 16; indexInSubChunk++)
                    {
                        //todo use a reasonable value instead of the hard coded 2
                        if (chunk.Parameters.ChunkInsideTerrain.GetBit(subChunkIndex))
                        {
                            chunk[subChunkIndex * 16 + indexInSubChunk] = new PackedDistanceFieldData(-2);
                        }
                        else
                        {
                            chunk[subChunkIndex * 16 + indexInSubChunk] = new PackedDistanceFieldData(2);
                        }
                    }
                }
            }
        }

        private static void GetCoverageAndFillMaskFromSurfaceDistance(NativeArray<PackedDistanceFieldData> result, out byte mask, out byte insideTerrainMask)
        {
            mask = 0;
            insideTerrainMask = 0;
            for (var i = 0; i < 2; i++)
            {
                var surfaceDistance = result[i].SurfaceDistance;

                for (var j = 0; j < 4; j++)
                {
                    //it should be possible to set this tolerance smaller
                    if (surfaceDistance.PackedValues[j] < 5 && surfaceDistance.PackedValues[j] > -5)
                    {
                        mask |= (byte) (1 << (i * 4 + j));
                    }

                    if (surfaceDistance.PackedValues[j] < 0)
                    {
                        insideTerrainMask |= (byte) (1 << (i * 4 + j));
                    }
                }
            }
        }

        // Returns a temp array of the positions that actually need to be computed
        private static void CreatePositionsArray(GeometryChunk chunk, out NativeArray<PackedFloat3> positions)
        {
            var countBits = math.countbits((int) chunk.Parameters.InnerDataMask);

            positions = new NativeArray<PackedFloat3>(16 * countBits, Allocator.Temp);
            new NativeArray<int>(16 * countBits, Allocator.Temp);

            var writtenPositionCount = 0;
            for (var subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
            {
                if (chunk.Parameters.InnerDataMask.GetBit(subChunkIndex))
                {
                    for (var indexInSubChunk = 0; indexInSubChunk < 16; indexInSubChunk++)
                    {
                        var positionWS = IndexToPositionWSPacked(subChunkIndex, indexInSubChunk,
                            chunk.Parameters.PositionWS);
                        positions[writtenPositionCount + indexInSubChunk] = positionWS;
                    }

                    writtenPositionCount += 16;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PackedFloat3 IndexToPositionWSPacked(int subChunkIndex, int indexInSubChunk, in float3 positionWS)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (subChunkIndex < 0 || subChunkIndex >= 8)
            {
                throw new ArgumentOutOfRangeException($"subChunkIndex is {subChunkIndex}, must be between 0 and 7");
            }

            if (indexInSubChunk < 0 || indexInSubChunk >= 16)
            {
                throw new ArgumentOutOfRangeException($"indexInSubChunk is {indexInSubChunk}, must be between 0 and 15");
            }
#endif

            var x = new float4(0, 1, 2, 3);
            float4 y = indexInSubChunk % 4;
            float4 z = indexInSubChunk / 4;

            x += subChunkIndex % 2 * 4;
            y += subChunkIndex / 2 % 2 * 4;
            z += subChunkIndex / 4 * 4;

            return new PackedFloat3(x, y, z) + positionWS;
        }
    }
}