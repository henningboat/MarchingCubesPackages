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

        [ReadOnly] public NativeList<Entity> DirtyEntities;
        [ReadOnly] public DynamicBuffer<GeometryInstruction> Instructions;
        [ReadOnly] public ComponentDataFromEntity<CGeometryChunk> GetChunkData;

         public ReadbackHandler ReadbackHandler;
        
        private void UpdateForEntity(CGeometryChunk chunkParameters, DynamicBuffer<GeometryInstruction> instructions,
            DynamicBuffer<PackedDistanceFieldData> distanceFieldData, Entity entity)
        {
            NativeArray<PackedFloat3> postionsWs;
            GeometryInstructionIterator distanceFieldResolver;
#if !SKIP_PREPASS

            var newPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
            var previousPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);

            var positions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
            var results = new NativeList<PackedDistanceFieldData>(PositionListCapacityEstimate, Allocator.Temp);

            previousPositions.Add(new MortonCoordinate(0));

            var layer = new MortonCellLayer(CellLength);

            while (layer.CellLength > 2)
            {
                newPositions.Clear();

                results.ResizeUninitialized(positions.Length);

                //todo refactor and make this nice
                NativeList<MortonCoordinate> mortonCoordinates = previousPositions;
                postionsWs = new NativeArray<PackedFloat3>(mortonCoordinates.Length, Allocator.Temp);
                for (var i = 0; i < mortonCoordinates.Length; i++)
                {
                    var mortonCoordinate = mortonCoordinates[i];
                    postionsWs[i] = layer.GetMortonCellChildPositions(mortonCoordinate) +
                                    chunkParameters.PositionWS;
                }

                distanceFieldResolver = new GeometryInstructionIterator(previousPositions, instructions, layer,
                    chunkParameters.PositionWS,GetPackedDistanceFieldBufferFromEntity, entity, postionsWs, false);

                postionsWs.Dispose();
                
                distanceFieldResolver.ProcessAllInstructions();


                for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
                {
                    var parentPosition = previousPositions[parentCellIndex];

                    ResolveChildCells(distanceFieldData);

                    void ResolveChildCells(DynamicBuffer<PackedDistanceFieldData> buffer)
                    {
                        var distance = distanceFieldResolver
                            ._terrainDataBuffer[parentCellIndex].SurfaceDistance;
                        var distanceInRange = SimdMath.abs(distance) < layer.CellLength * 1.25F;

                        distanceInRange = new bool8(true,true);
                        
                        for (uint i = 0; i < 8; i++)
                        {
                            var childMortonNumber = layer.GetChildCell(parentPosition, i);
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


            results.ResizeUninitialized(previousPositions.Length);
            
          
            postionsWs = new NativeArray<PackedFloat3>(previousPositions.Length, Allocator.Temp);
            for (var i = 0; i < previousPositions.Length; i++)
            {
                var mortonCoordinate = previousPositions[i];
                postionsWs[i] = layer.GetMortonCellChildPositions(mortonCoordinate) +
                                chunkParameters.PositionWS;
            }
            #else
            postionsWs = new NativeArray<PackedFloat3>(Constants.chunkVolume/Constants.PackedCapacity, Allocator.Temp);

            var layer = new MortonCellLayer(2);
            for (int i = 0; i < 64; i++)
            {
                postionsWs[i] = layer.GetMortonCellChildPositions(new MortonCoordinate((uint)i))+chunkParameters.PositionWS;;
            }

            ReadbackHandler.SetEntityIndex(chunkParameters.IndexInIndexMap);

            distanceFieldResolver =
                new GeometryInstructionIterator(default, instructions.AsNativeArray(), default, postionsWs, false, ReadbackHandler);
            distanceFieldResolver.ProcessAllInstructions();

            distanceFieldData.AsNativeArray().Slice(0,distanceFieldData.Length)
                .CopyFrom(distanceFieldResolver._terrainDataBuffer.Slice(0,distanceFieldData.Length));
#endif
#if !SKIP_PREPASS
            for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
            {
                var parentPosition = previousPositions[parentCellIndex];

                var index = parentPosition.MortonNumber / 8;
                distanceFieldData[(int) index] =
                    distanceFieldResolver._terrainDataBuffer[parentCellIndex];
            }


            newPositions.Dispose();
            previousPositions.Dispose();
            positions.Dispose();
            results.Dispose();
            
            distanceFieldResolver.Dispose();
#else
            postionsWs.Dispose();
            distanceFieldResolver.Dispose();
#endif
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
            var distanceField = ReadbackHandler.GetPackDistanceFieldData[chunkEntity];

            UpdateForEntity(GetChunkData[chunkEntity], Instructions, distanceField, chunkEntity);
        }
    }
}