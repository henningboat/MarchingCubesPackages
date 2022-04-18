// using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
// using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
//
// namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
// {
//     [BurstCompile]
//     public struct JExecuteDistanceFieldPrepass : IJobParallelFor
//     {
//         [ReadOnly] private NativeArray<GeometryInstruction> _geometryInstructions;
//         private DistanceDataReadbackCollection _data;
//         private readonly int _layerIndex;
//
//         public JExecuteDistanceFieldPrepass(DistanceDataReadbackCollection data, int layerIndex,
//             NativeArray<GeometryInstruction> geometryInstructions)
//         {
//             _layerIndex = layerIndex;
//             _data = data;
//             _geometryInstructions = geometryInstructions;
//         }
//
//         public void Execute(int clusterIndex)
//         {
//             var geometryFieldData = _data[_layerIndex];
//             var cluster = geometryFieldData.GetCluster(clusterIndex);
//
//             var hashPerChunk = new NativeArray<Hash128>(Constants.chunksPerCluster, Allocator.Temp);
//
//             //there's probably a better way to do this, but for now this ensures that a chunk with no instructions
//             //still does not get a has of zero, which forces it to still be initialized
//             var defaultHash = Hash128.Compute(2.3278103498234f);
//             for (var i = 0; i < Constants.chunksPerCluster; i++) hashPerChunk[i] = defaultHash;
//
//             var indicesInCluster = new NativeArray<int>(512, Allocator.Temp);
//
//
//             var middleIndexWithinChunk = 63;
//             for (var i = 0; i < Constants.chunksPerCluster; i++)
//             {
//                 var chunk = cluster.GetChunk(i);
//                 indicesInCluster[i] = chunk.Parameters.IndexInCluster * Constants.chunkVolume + middleIndexWithinChunk;
//             }
//
//             var iterator =
//                 new GeometryInstructionIterator(cluster, indicesInCluster, _geometryInstructions, true, _data);
//
//             indicesInCluster.Dispose();
//
//             var instructionIndex = 0;
//
//             // if (geometryFieldData.GeometryLayer.ClearEveryFrame == false)
//             // {
//             //     iterator.ProcessTerrainData(0);
//             //     //if the layer reads it's own frame, we don't want to read our own hash from last frame
//             //     instructionIndex++;
//             // }
//
//             for (; instructionIndex < _geometryInstructions.Length; instructionIndex++)
//             {
//                 iterator.ProcessTerrainData(instructionIndex);
//                 var currentGeometryInstruction = _geometryInstructions[instructionIndex];
//                 for (var i = 0; i < iterator.CurrentInstructionSurfaceDistanceReadback.Length; i++)
//                 {
//                     var distance = iterator.CurrentInstructionSurfaceDistanceReadback[i].PackedValues;
//
//                     //todo turn back to 10
//                     const int prepassDistance = 12;
//
//                     var isWriting = (distance < prepassDistance) & (distance > -prepassDistance) &
//                                     currentGeometryInstruction.WritesToDistanceField;
//                     for (var k = 0; k < 4; k++)
//                         if (isWriting[k])
//                         {
//                             var chunkIndex = k + 4 * i;
//                             var hash128 = hashPerChunk[chunkIndex];
//
//                             Hash128 hashOfInstruction;
//                             if (currentGeometryInstruction.GeometryInstructionType == GeometryInstructionType.CopyLayer)
//                             {
//                                  var readbackCluster = _data[currentGeometryInstruction.GeometryInstructionSubType]
//                                      .GetCluster(clusterIndex);
//                                  if (readbackCluster.Parameters.WriteMask[chunkIndex] == false) continue;
//                                 
//                                  hashOfInstruction = currentGeometryInstruction.GeometryInstructionHash;
//                                 
//                                  //only if we don't currently read from our own layer, we want to add the current hash of the
//                                  //read content to the mix. Else, we would get a new has everytime
//                                  if (i != 0 || geometryFieldData.GeometryLayer.ClearEveryFrame)
//                                  {
//                                      var currentHashOnReadbackLayer = readbackCluster.GetChunk(chunkIndex).Parameters
//                                          .CurrentGeometryInstructionsHash;
//                                      hashOfInstruction.Append(ref currentHashOnReadbackLayer);
//                                  }
//                             }
//                             else
//                             {
//                                 hashOfInstruction = currentGeometryInstruction.GeometryInstructionHash;
//                             }
//
//                             hash128.Append(ref hashOfInstruction);
//                             hashPerChunk[chunkIndex] = hash128;
//                         }
//                 }
//             }
//
//             var clusterParameters = cluster.Parameters;
//
//             for (var chunkIndex = 0; chunkIndex < 512; chunkIndex++)
//             {
//                 var newInstructionHash = hashPerChunk[chunkIndex];
//                 var writeMaskValue = newInstructionHash != defaultHash;
//
//
//                 clusterParameters.WriteMask[chunkIndex] = writeMaskValue;
//
//                 var chunk = cluster.GetChunk(chunkIndex);
//                 var chunkParameters = chunk.Parameters;
//                 chunkParameters.InstructionsChangedSinceLastFrame =
//                     newInstructionHash != chunkParameters.CurrentGeometryInstructionsHash;
//                 chunkParameters.CurrentGeometryInstructionsHash = newInstructionHash;
//
//                 chunk.Parameters = chunkParameters;
//             }
//
//             hashPerChunk.Dispose();
//
//             cluster.Parameters = clusterParameters;
//         }
//     }
// }