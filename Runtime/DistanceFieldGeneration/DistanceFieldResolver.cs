using System;
using System.Runtime.CompilerServices;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils;
using SIMDMath;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public static class DistanceFieldResolver
    {
        public static void CalculateDistanceFieldForChunk(GeometryCluster cluster, GeometryChunk chunk, NativeArray<GeometryInstruction> geometryInstructions, bool clearEveryFrame)
        {
            var clusterParameters = cluster.Parameters;
            var chunkParameters = chunk.Parameters;

            //todo
            if (clusterParameters.WriteMask[chunkParameters.IndexInCluster]&& chunkParameters.InstructionsChangedSinceLastFrame)
            {
                // var positionsToCheck = new NativeArray<PackedFloat3>(2, Allocator.Temp);
                // var currentDistanceValue = new NativeArray<PackedDistanceFieldData>(2, Allocator.Temp);
                //
                // for (var i = 0; i < 2; i++)
                // {
                //     int4 offsetX = new int4(2, 6, 2, 6);
                //     int4 offsetY = new int4(2, 2, 6, 6);
                //     int4 offsetZ = new int4(i) * 4 + 2;
                //
                //
                //     var packedOffsetInChunk = new PackedFloat3(offsetX, offsetY, offsetZ);
                //     positionsToCheck[i] = new PackedFloat3(chunkParameters.PositionWS) + packedOffsetInChunk;
                //
                //     var packedReadBackValue = new PackedDistanceFieldData();
                //
                //     //super ugly and probably costs a bunch of performance
                //     for (int j = 0; j < 4; j++)
                //     {
                //         int3 offsetInChunk = new int3(offsetX[j], offsetY[j], offsetZ[j]);
                //         offsetInChunk = 3;
                //
                //         int subChunkIndex = Utils.PositionToIndex(offsetInChunk / 4, 2);
                //         int indexInSubChunk = Utils.PositionToIndex(offsetInChunk%4, 4);
                //
                //         int indexInChunk = subChunkIndex * Constants.chunkVolume / Constants.subChunksPerChunk + indexInSubChunk;
                //
                //         packedReadBackValue.SurfaceDistance.PackedValues[j] =
                //             chunk[indexInChunk / 4].SurfaceDistance.PackedValues[indexInChunk % 4];
                //     }
                //
                //     currentDistanceValue[i] = packedReadBackValue;
                // }
                //
                // var iterator = new GeometryInstructionIterator(positionsToCheck, geometryInstructions, clearEveryFrame, false,currentDistanceValue);
                //
                // iterator.CalculateAllTerrainData();
                //
                // GetCoverageAndFillMaskFromSurfaceDistance(iterator._terrainDataBuffer, out var mask, out var insideTerrainMask);
                //
                // chunkParameters.InnerDataMask = mask;
                // chunkParameters.ChunkInsideTerrain = insideTerrainMask;
                //
                // positionsToCheck.Dispose();
                // currentDistanceValue.Dispose();

                chunkParameters.InnerDataMask = 255;
                chunkParameters.ChunkInsideTerrain = 255;
            }
            else
            {
                chunkParameters.InnerDataMask = 0;
                return;
            }

            chunk.Parameters = chunkParameters;
            
            
            GeometryInstructionIterator detailIterator=default;
            var containsDetails = chunkParameters.InnerDataMask != 0;
            if (containsDetails)
            {
                CreatePositionsArray(chunk, out var positions, out var readbackValues);

                detailIterator =
                    new GeometryInstructionIterator(positions, geometryInstructions, clearEveryFrame, false, readbackValues);
                detailIterator.CalculateAllTerrainData();

                readbackValues.Dispose();
            }
            
            
            CopyResultsBackToBuffer(chunk, detailIterator);
            
            if (containsDetails)
            {
                detailIterator.Dispose();
            }

        }

        private static void CopyResultsBackToBuffer(GeometryChunk chunk, GeometryInstructionIterator iterator)
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
        private static void CreatePositionsArray(GeometryChunk chunk, out NativeArray<PackedFloat3> positions,
            out NativeArray<PackedDistanceFieldData> distanceFieldReadback)
        {
            var countBits = math.countbits((int) chunk.Parameters.InnerDataMask);

            positions = new NativeArray<PackedFloat3>(16 * countBits, Allocator.Temp);
            distanceFieldReadback = new NativeArray<PackedDistanceFieldData>(16 * countBits, Allocator.Temp);

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

                        distanceFieldReadback[writtenPositionCount + indexInSubChunk] = chunk[
                            subChunkIndex * ((Constants.chunkVolume / Constants.subChunksPerChunk) / Constants.PackedCapacity) + indexInSubChunk];
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