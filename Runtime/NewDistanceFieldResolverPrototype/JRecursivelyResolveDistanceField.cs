using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils;
using SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.NewDistanceFieldResolverPrototype
{
    public struct GeometryChunk
    {
        private const int length = 16;
        public NativeSlice<PackedDistanceFieldData> GeometryFieldBuffer;
        public NativeSlice<ulong> CoverageMask;
    }
    [BurstCompile]
    public struct JRecursivelyResolveDistanceField : IJob
    {
        public NativeArray<PackedDistanceFieldData> GeometryFieldBuffer;
        public NativeArray<GeometryInstruction> Instructions;

        public float Time;

        private const int positionListCapacityEstimate = 4096;

        public void Execute()
        {
            var newPositions = new NativeList<MortonCoordinate>(positionListCapacityEstimate, Allocator.Temp);
            var previousPositions = new NativeList<MortonCoordinate>(positionListCapacityEstimate, Allocator.Temp);

            var positions = new NativeList<PackedFloat3>(positionListCapacityEstimate, Allocator.Temp);
            var results = new NativeList<PackedDistanceFieldData>(positionListCapacityEstimate, Allocator.Temp);

            previousPositions.Add(new MortonCoordinate(0));

            var layer = new MortonCellLayer(64);

            GeometryInstructionIterator distanceFieldResolver;
            while (layer.CellLength > 2)
            {
                newPositions.Clear();

                FillPositionsIntoPositionBuffer(previousPositions, layer, positions);
                results.ResizeUninitialized(positions.Length);

                distanceFieldResolver = new GeometryInstructionIterator(positions, Instructions, default);
                distanceFieldResolver.CalculateAllTerrainData();


                for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
                {
                    var parentPosition = previousPositions[parentCellIndex];

                    ResolveChildCells(false, GeometryFieldBuffer);
                    ResolveChildCells(true, GeometryFieldBuffer);

                    void ResolveChildCells(bool secondRow,
                        NativeArray<PackedDistanceFieldData> buffer)
                    {
                        var distance = distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + (secondRow ? 1 : 0)].SurfaceDistance;
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
            
            
            
            
            
            FillPositionsIntoPositionBuffer(previousPositions, layer, positions);
            results.ResizeUninitialized(positions.Length);

            distanceFieldResolver = new GeometryInstructionIterator(positions, Instructions, default);
            distanceFieldResolver.CalculateAllTerrainData();


            for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
            {
                var parentPosition = previousPositions[parentCellIndex];

                var index = parentPosition.MortonNumber / 4;
                GeometryFieldBuffer[(int) index + 0] =
                    distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 0];
                GeometryFieldBuffer[(int) index + 1] =
                    distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 1];
            }


            distanceFieldResolver.Dispose();
            
            newPositions.Dispose();
            previousPositions.Dispose();
            positions.Dispose();
            results.Dispose();
        }

        private void FillPositionsIntoPositionBuffer(NativeList<MortonCoordinate> previousPositions,
            MortonCellLayer mortonLayer, NativeList<PackedFloat3> positions)
        {
            positions.Clear();
            for (int i = 0; i < previousPositions.Length; i++)
            {
                positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], false));
                positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], true));
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