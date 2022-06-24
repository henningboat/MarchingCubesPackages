// using henningboat.CubeMarching.Runtime.Components;
// using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
// using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
// using henningboat.CubeMarching.Runtime.Utils;
// using SIMDMath;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
//
// namespace henningboat.CubeMarching.Runtime.Systems
// {
//     [BurstCompile]
//     public partial struct JPrepassGeometryChunks : IJobEntityBatch
//     {
//         public const int CellLength = Constants.chunkLength;
//         private const int PositionListCapacityEstimate = 4096;
//         [ReadOnly] public BufferFromEntity<GeometryInstruction> GetInstructionsFromEntity;
//         [ReadOnly] public ComponentTypeHandle<CGeometryChunk> GeometryChunkTypeHandle;
//         [ReadOnly] public ComponentTypeHandle<CGeometryLayerReference> CGeometryLayerReferenceHandle;
//         public BufferTypeHandle<PackedDistanceFieldData> PackedDistanceFieldDataHandle;
//
//         public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
//         {
//             var layerEntity = batchInChunk.GetChunkComponentData<CGeometryLayerReference>(CGeometryLayerReferenceHandle)
//                 .LayerEntity;
//
//             var instructions = GetInstructionsFromEntity[layerEntity].AsNativeArray();
//
//             var distanceFieldAccessor = batchInChunk.GetBufferAccessor(PackedDistanceFieldDataHandle);
//
//             var chunkParameters = batchInChunk.GetNativeArray(GeometryChunkTypeHandle);
//             
//             var newPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
//             var previousPositions = new NativeList<MortonCoordinate>(PositionListCapacityEstimate, Allocator.Temp);
//             
//             var positions = new NativeList<PackedFloat3>(PositionListCapacityEstimate, Allocator.Temp);
//             var results = new NativeList<PackedDistanceFieldData>(PositionListCapacityEstimate, Allocator.Temp);
//
//
//
//             for (var i = 0; i < batchInChunk.Count; i++)
//             {
//                 newPositions.Clear();
//                 previousPositions.Clear();
//                 positions.Clear();
//                 results.Clear();
//                 
//                 var distanceFieldData = distanceFieldAccessor[i].AsNativeArray();
//                 UpdateForEntity(chunkParameters[i], previousPositions, positions, results, instructions,
//                     distanceFieldData, newPositions);
//             }
//
//             newPositions.Dispose();
//             previousPositions.Dispose();
//             positions.Dispose();
//             results.Dispose();
//         }
//
//         private void UpdateForEntity(CGeometryChunk chunkParameters, NativeList<MortonCoordinate> previousPositions,
//             NativeList<PackedFloat3> positions,
//             NativeList<PackedDistanceFieldData> results, NativeArray<GeometryInstruction> instructions,
//             NativeArray<PackedDistanceFieldData> distanceFieldData, NativeList<MortonCoordinate> newPositions)
//         {
//             previousPositions.Add(new MortonCoordinate(0));
//
//             var layer = new MortonCellLayer(CellLength);
//
//             GeometryInstructionIterator distanceFieldResolver;
//             while (layer.CellLength > 2)
//             {
//                 newPositions.Clear();
//
//                 FillPositionsIntoPositionBuffer(previousPositions, layer, positions, chunkParameters);
//                 results.ResizeUninitialized(positions.Length);
//
//                 distanceFieldResolver = new GeometryInstructionIterator(positions, instructions, default);
//                 distanceFieldResolver.CalculateAllTerrainData();
//
//
//                 for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
//                 {
//                     var parentPosition = previousPositions[parentCellIndex];
//
//                     ResolveChildCells(false, distanceFieldData);
//                     ResolveChildCells(true, distanceFieldData);
//
//                     void ResolveChildCells(bool secondRow,
//                         NativeArray<PackedDistanceFieldData> buffer)
//                     {
//                         var distance = distanceFieldResolver
//                             ._terrainDataBuffer[parentCellIndex * 2 + (secondRow ? 1 : 0)].SurfaceDistance;
//                         var distanceInRange = SimdMath.abs(distance).PackedValues < layer.CellLength * 1.25F;
//
//                         for (uint i = 0; i < 4; i++)
//                         {
//                             var childMortonNumber = layer.GetChildCell(parentPosition, secondRow ? i + 4 : i);
//                             if (distanceInRange[(int) i])
//                                 newPositions.Add(childMortonNumber);
//                             else
//                                 WriteDistanceToCellInBuffer(buffer, childMortonNumber, layer,
//                                     distance.PackedValues[(int) i]);
//                         }
//                     }
//                 }
//
//                 distanceFieldResolver.Dispose();
//                 layer = layer.GetChildLayer();
//
//                 if (newPositions.Length == 0)
//                 {
//                     return;
//                 }
//                 
//                 (previousPositions, newPositions) = (newPositions, previousPositions);
//             }
//
//
//             FillPositionsIntoPositionBuffer(previousPositions, layer, positions, chunkParameters);
//             results.ResizeUninitialized(positions.Length);
//
//             distanceFieldResolver = new GeometryInstructionIterator(positions, instructions, default);
//             distanceFieldResolver.CalculateAllTerrainData();
//
//
//             for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
//             {
//                 var parentPosition = previousPositions[parentCellIndex];
//
//                 var index = parentPosition.MortonNumber / 4;
//                 distanceFieldData[(int) index + 0] =
//                     distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 0];
//                 distanceFieldData[(int) index + 1] =
//                     distanceFieldResolver._terrainDataBuffer[parentCellIndex * 2 + 1];
//             }
//
//
//             distanceFieldResolver.Dispose();
//         }
//
//         private void FillPositionsIntoPositionBuffer(NativeList<MortonCoordinate> previousPositions,
//             MortonCellLayer mortonLayer, NativeList<PackedFloat3> positions, CGeometryChunk geometryChunk)
//         {
//             positions.Clear();
//             for (var i = 0; i < previousPositions.Length; i++)
//             {
//                 positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], false)+geometryChunk.PositionWS);
//                 positions.Add(mortonLayer.GetMortonCellChildPositions(previousPositions[i], true) + geometryChunk.PositionWS);
//             }
//         }
//
//         private static void WriteDistanceToCellInBuffer(NativeArray<PackedDistanceFieldData> buffer,
//             MortonCoordinate childMortonNumber, MortonCellLayer layer, float distancePackedValue)
//         {
//             var distanceFieldData = new PackedDistanceFieldData(distancePackedValue);
//
//             for (var i = 0; i < layer.CellPackedBufferSize / 8; i++)
//                 buffer[(int) (childMortonNumber.MortonNumber / 4 + i)] = distanceFieldData;
//         }
//     }
// }