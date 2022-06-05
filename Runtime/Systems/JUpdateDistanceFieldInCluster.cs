﻿using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils;
using SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [BurstCompile]
    public partial struct JUpdateDistanceFieldInCluster : IJobEntity
    {
        public const int CellLength = Constants.chunkLength;
        private const int PositionListCapacityEstimate = 4096;
        [ReadOnly] public BufferFromEntity<GeometryInstruction> GetInstructionsFromEntity;

        public void Execute(in CGeometryChunk chunkParameters,
            DynamicBuffer<PackedDistanceFieldData> distanceFieldDynamicBuffer)
        {
            var instructions = GetInstructionsFromEntity[chunkParameters.LayerEntity].AsNativeArray();

            var distanceFieldData = distanceFieldDynamicBuffer.AsNativeArray();

            var newPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
            var previousPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);

            var positions = new NativeList<PackedFloat3>(PositionListCapacityEstimate, Allocator.Temp);
            var results = new NativeList<PackedDistanceFieldData>(PositionListCapacityEstimate, Allocator.Temp);

            previousPositions.Add(new MortonCoordinate(0));

            var layer = new MortonCellLayer(CellLength);

            GeometryInstructionIterator distanceFieldResolver;
            while (layer.CellLength > 2)
            {
                newPositions.Clear();

                FillPositionsIntoPositionBuffer(previousPositions, layer, positions, chunkParameters);
                results.ResizeUninitialized(positions.Length);

                distanceFieldResolver = new GeometryInstructionIterator(positions, instructions, default);
                distanceFieldResolver.CalculateAllTerrainData();


                for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
                {
                    var parentPosition = previousPositions[parentCellIndex];

                    ResolveChildCells(false, distanceFieldData);
                    ResolveChildCells(true, distanceFieldData);

                    void ResolveChildCells(bool secondRow,
                        NativeArray<PackedDistanceFieldData> buffer)
                    {
                        var distance = distanceFieldResolver
                            ._terrainDataBuffer[parentCellIndex * 2 + (secondRow ? 1 : 0)].SurfaceDistance;
                        var distanceInRange = SimdMath.abs(distance).PackedValues < layer.CellLength * 1.25F;

                        for (uint i = 0; i < 4; i++)
                        {
                            var childMortonNumber = layer.GetChildCell(parentPosition, secondRow ? i + 4 : i);
                            if (distanceInRange[(int) i])
                                newPositions.Add(childMortonNumber);
                            else
                                WriteDistanceToCellInBuffer(buffer, childMortonNumber, layer,
                                    distance.PackedValues[(int) i]);
                        }
                    }
                }

                distanceFieldResolver.Dispose();
                layer = layer.GetChildLayer();

                (previousPositions, newPositions) = (newPositions, previousPositions);
            }


            FillPositionsIntoPositionBuffer(previousPositions, layer, positions, chunkParameters);
            results.ResizeUninitialized(positions.Length);

            distanceFieldResolver = new GeometryInstructionIterator(positions, instructions, default);
            distanceFieldResolver.CalculateAllTerrainData();


            for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
            {
                var parentPosition = previousPositions[parentCellIndex];

                var index = parentPosition.MortonNumber / 4;
                distanceFieldData[(int) index + 0] =
                    distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 0];
                distanceFieldData[(int) index + 1] =
                    distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 1];
            }


            distanceFieldResolver.Dispose();

            newPositions.Dispose();
            previousPositions.Dispose();
            positions.Dispose();
            results.Dispose();
        }

        private void FillPositionsIntoPositionBuffer(NativeList<MortonCoordinate> previousPositions,
            MortonCellLayer mortonLayer, NativeList<PackedFloat3> positions, CGeometryChunk geometryChunk)
        {
            positions.Clear();
            for (var i = 0; i < previousPositions.Length; i++)
            {
                positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], false)+geometryChunk.PositionWS);
                positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], true) + geometryChunk.PositionWS);
            }
        }

        private static void WriteDistanceToCellInBuffer(NativeArray<PackedDistanceFieldData> buffer,
            MortonCoordinate childMortonNumber, MortonCellLayer layer, float distancePackedValue)
        {
            var distanceFieldData = new PackedDistanceFieldData(distancePackedValue);
            for (var i = 0; i < layer.CellPackedBufferSize / 8; i++)
                buffer[(int) (childMortonNumber.MortonNumber / 4 + i)] = distanceFieldData;
        }
    }
}