using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils;
using SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [BurstCompile]
    public partial struct JUpdateDistanceField : IJobParallelFor
    {
        public const int CellLength = Constants.chunkLength;
        private const int PositionListCapacityEstimate = 4096;
        
        [NativeDisableParallelForRestriction,NativeDisableContainerSafetyRestriction]public BufferFromEntity<PackedDistanceFieldData> GetPackedDistanceFieldBufferFromEntity;

        [ReadOnly] public NativeList<Entity> DirtyEntities;
        [ReadOnly] public DynamicBuffer<GeometryInstruction> Instructions;
        [ReadOnly] public ComponentDataFromEntity<CGeometryChunk> GetChunkData;

        private void UpdateForEntity(CGeometryChunk chunkParameters, DynamicBuffer<GeometryInstruction> instructions,
            DynamicBuffer<PackedDistanceFieldData> distanceFieldData, Entity entity)
        {
            
            var newPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
            var previousPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);

            var positions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
            var results = new NativeList<PackedDistanceFieldData>(PositionListCapacityEstimate, Allocator.Temp);

            previousPositions.Add(new MortonCoordinate(0));
            NativeArray<PackedFloat3> postionsWs;

            var layer = new MortonCellLayer(CellLength);

            GeometryInstructionIterator distanceFieldResolver;
            while (layer.CellLength > 2)
            {
                newPositions.Clear();

                results.ResizeUninitialized(positions.Length);

                //todo refactor and make this nice
                var mortonCoordinates = previousPositions;
                postionsWs = new NativeArray<PackedFloat3>(mortonCoordinates.Length * 2, Allocator.Temp);
                for (var i = 0; i < mortonCoordinates.Length; i++)
                {
                    var mortonCoordinate = mortonCoordinates[i];
                    postionsWs[i * 2 + 0] = layer.GetMortonCellChildPositions(mortonCoordinate, false) +
                                             chunkParameters.PositionWS;
                    postionsWs[i * 2 + 1] = layer.GetMortonCellChildPositions(mortonCoordinate, true) +
                                             chunkParameters.PositionWS;
                }

                
                distanceFieldResolver = new GeometryInstructionIterator(previousPositions, instructions, layer,
                    chunkParameters.PositionWS,GetPackedDistanceFieldBufferFromEntity, entity, postionsWs);

                postionsWs.Dispose();
                
                distanceFieldResolver.CalculateAllTerrainData();


                for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
                {
                    var parentPosition = previousPositions[parentCellIndex];

                    ResolveChildCells(false, distanceFieldData);
                    ResolveChildCells(true, distanceFieldData);

                    void ResolveChildCells(bool secondRow,
                        DynamicBuffer<PackedDistanceFieldData> buffer)
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

                if (newPositions.Length == 0)
                {
                    return;
                }
                
                (previousPositions, newPositions) = (newPositions, previousPositions);
            }


            results.ResizeUninitialized(previousPositions.Length * 2);
            
            postionsWs = new NativeArray<PackedFloat3>(previousPositions.Length * 2, Allocator.Temp);
            for (var i = 0; i < previousPositions.Length; i++)
            {
                var mortonCoordinate = previousPositions[i];
                postionsWs[i * 2 + 0] = layer.GetMortonCellChildPositions(mortonCoordinate, false) +
                                        chunkParameters.PositionWS;
                postionsWs[i * 2 + 1] = layer.GetMortonCellChildPositions(mortonCoordinate, true) +
                                        chunkParameters.PositionWS;
            }

            
            distanceFieldResolver =
                new GeometryInstructionIterator(previousPositions, instructions, layer, chunkParameters.PositionWS,GetPackedDistanceFieldBufferFromEntity,entity, postionsWs);
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


            newPositions.Dispose();
            previousPositions.Dispose();
            positions.Dispose();
            results.Dispose();
            
            distanceFieldResolver.Dispose();
        }

        // private void FillPositionsIntoPositionBuffer(NativeList<MortonCoordinate> previousPositions,
        //     MortonCellLayer mortonLayer, NativeList<PackedFloat3> positions, CGeometryChunk geometryChunk)
        // {
        //     positions.Clear();
        //     for (var i = 0; i < previousPositions.Length; i++)
        //     {
        //         positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], false)+geometryChunk.PositionWS);
        //         positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], true) + geometryChunk.PositionWS);
        //     }
        // }

        private static void WriteDistanceToCellInBuffer(DynamicBuffer<PackedDistanceFieldData> buffer,
            MortonCoordinate childMortonNumber, MortonCellLayer layer, float distancePackedValue)
        {
            var distanceFieldData = new PackedDistanceFieldData(distancePackedValue);

            for (var i = 0; i < layer.CellPackedBufferSize / 8; i++)
                buffer[(int) (childMortonNumber.MortonNumber / 4 + i)] = distanceFieldData;
        }

        public void Execute(int index)
        {
            if (index >= DirtyEntities.Length)
            {
                return;
            }

            var chunkEntity = DirtyEntities[index];
            var distanceField = GetPackedDistanceFieldBufferFromEntity[chunkEntity];

            UpdateForEntity(GetChunkData[chunkEntity], Instructions, distanceField, chunkEntity);
        }
    }
}