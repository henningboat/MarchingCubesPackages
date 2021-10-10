﻿// using Code.CubeMarching.Rendering;
// using Code.CubeMarching.TerrainChunkEntitySystem;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
//
// namespace Code.CubeMarching.StateHashing
// {
//     public struct GeometryInstructionsHasher
//     {
//         // private readonly BufferFromEntity<GeometryInstruction> _getTerrainInstructions;
//         // private readonly ComponentDataFromEntity<CClusterPosition> _getClusterPosition;
//         //
//         // public GeometryInstructionsHasher(SystemBase systemBase)
//         // {
//         //     _getTerrainInstructions = systemBase.GetBufferFromEntity<GeometryInstruction>(true);
//         //     _getClusterPosition = systemBase.GetComponentDataFromEntity<CClusterPosition>();
//         // }
//         //
//         // public void Execute(ref DistanceFieldChunkData chunk, in CTerrainEntityChunkPosition chunkPosition, in CClusterParameters clusterParameters, in ClusterChild clusterChild, int frameCount)
//         // {
//         //     uint hash = 0;
//         //     var instructions = _getTerrainInstructions[clusterChild.ClusterEntity].Reinterpret<GeometryInstruction>();
//         //     var clusterPosition = _getClusterPosition[clusterChild.ClusterEntity];
//         //
//         //     var indexInCluster = chunkPosition.indexInCluster;
//         //     
//         //     if(clusterParameters.WriteMask[indexInCluster])
//         //     {
//         //         foreach (var terrainInstruction in instructions)
//         //         {
//         //             hash.AddToHash(terrainInstruction.Hash);
//         //         }
//         //     }
//         //
//         //     var hashChanged = chunk.CurrentGeometryInstructionsHash != hash;
//         //     hashChanged = true;
//         //     chunk.InstructionsChangedSinceLastFrame = hashChanged;
//         //     if (hashChanged)
//         //     {
//         //         chunk.InstructionChangeFrameCount = frameCount;
//         //     }
//         //
//         //     chunk.CurrentGeometryInstructionsHash = hash;
//         // }
//     }
// }