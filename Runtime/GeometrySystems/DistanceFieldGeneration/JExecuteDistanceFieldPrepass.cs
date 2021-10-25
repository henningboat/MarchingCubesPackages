using Code.SIMDMath;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    public struct JExecuteDistanceFieldPrepass:IJobParallelFor
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphData _graph;
        private Random _random;

        public JExecuteDistanceFieldPrepass(GeometryFieldData geometryFieldData, GeometryGraphData graph)
        {
            _graph = graph;
            _geometryFieldData = geometryFieldData;
            _random = new Random((uint)UnityEngine.Random.Range(0, 100000000));
        }

        public void Execute(int clusterIndex)
        {
            var cluster = _geometryFieldData.GetCluster(clusterIndex);
            NativeArray<PackedFloat3> positions =
                new NativeArray<PackedFloat3>(Constants.chunksPerCluster / Constants.PackedCapacity, Allocator.Temp);

            NativeArray<Hash128> hashPerChunk = new NativeArray<Hash128>(Constants.chunksPerCluster, Allocator.Temp);

            //somewhat unclean way to get a packed array of the center points of all 512 chunks in a cluster
            for (int i = 0; i < Constants.chunksPerCluster/Constants.PackedCapacity; i++)
            {
                PackedFloat3 position = new PackedFloat3(TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, new int3(2, 8, 8)));
                position *= 8;
                if (i % 2 == 0)
                {
                    position.x = new PackedFloat(0, 8, 16, 24);
                }
                else
                {
                    position.x = new PackedFloat(32, 40, 48, 56);
                }

                position += 3.5f;
                
                positions[i] = position;
            }
            //
             TerrainInstructionIterator iterator = new TerrainInstructionIterator(positions,_graph.GeometryInstructions,_graph.ValueBuffer,true);

             for (int i = 0; i < _graph.GeometryInstructions.Length; i++)
             {
                 iterator.ProcessTerrainData(i);
                 var currentGeometryInstruction = _graph.GeometryInstructions[i];
                 if (currentGeometryInstruction.WritesToDistanceField)
                 {
                     for (int j = 0; j < iterator.CurrentInstructionSurfaceDistanceReadback.Length; j++)
                     {
                         var distance = iterator.CurrentInstructionSurfaceDistanceReadback[j].PackedValues;
                         bool4 isWriting = distance < 10 & distance > -10;
                         for (int k = 0; k < 4; k++)
                         {
                             if (isWriting[k])
                             {
                                 var hash128 = hashPerChunk[k * 4 + j];
                                 var hashOfInstruction = _graph.HashPerInstruction[i];
                                 hash128.Append(ref hashOfInstruction);
                                 hashPerChunk[k + j * 4] = hash128;
                             }
                         }
                     }
                 }
             }

             var clusterParameters = cluster.Parameters;
            
             for (int i = 0; i < 512; i++)
             {
                 var newInstructionHash = hashPerChunk[i];
                 clusterParameters.WriteMask[i] = newInstructionHash != default;

                 var chunk = cluster.GetChunk(i);
                 var chunkParameters = chunk.Parameters;
                 chunkParameters.InstructionsChangedSinceLastFrame = newInstructionHash != chunkParameters.CurrentGeometryInstructionsHash;
                 chunkParameters.CurrentGeometryInstructionsHash = newInstructionHash;
                 
                 
                 clusterParameters.WriteMask[i] = true;
                 chunkParameters.InstructionsChangedSinceLastFrame = true;
                 
                 chunk.Parameters = chunkParameters;
             }

             hashPerChunk.Dispose();
             positions.Dispose();

             cluster.Parameters = clusterParameters;

             // throw new NotImplementedException();
        }
    }
}