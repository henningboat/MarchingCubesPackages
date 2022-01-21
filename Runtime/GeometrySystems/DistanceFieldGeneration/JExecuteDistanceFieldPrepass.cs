using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    public struct JExecuteDistanceFieldPrepass : IJobParallelFor
    {
        private GeometryFieldData _geometryFieldData;
        [ReadOnly] private NativeArray<GeometryInstruction> _geometryInstructions;

        public JExecuteDistanceFieldPrepass(GeometryFieldData geometryFieldData,
            NativeArray<GeometryInstruction> geometryInstructions)
        {
            _geometryInstructions = geometryInstructions;
            _geometryFieldData = geometryFieldData;
        }

        public void Execute(int clusterIndex)
        {
            var cluster = _geometryFieldData.GetCluster(clusterIndex);
            var positions =
                new NativeArray<PackedFloat3>(Constants.chunksPerCluster / Constants.PackedCapacity, Allocator.Temp);

            var hashPerChunk = new NativeArray<Hash128>(Constants.chunksPerCluster, Allocator.Temp);

            //somewhat unclean way to get a packed array of the center points of all 512 chunks in a cluster
            for (var i = 0; i < Constants.chunksPerCluster / Constants.PackedCapacity; i++)
            {
                var position =
                    new PackedFloat3(Runtime.DistanceFieldGeneration.Utils.IndexToPositionWS(i, new int3(2, 8, 8)));
                position *= 8;
                if (i % 2 == 0)
                    position.x = new PackedFloat(0, 8, 16, 24);
                else
                    position.x = new PackedFloat(32, 40, 48, 56);

                position += 3.5f;

                position += new PackedFloat3(cluster.Parameters.PositionWS);

                positions[i] = position;
            }

            var iterator =
                new GeometryInstructionIterator(positions, _geometryInstructions, _geometryFieldData.GeometryLayer.ClearEveryFrame, true);

            for (var i = 0; i < _geometryInstructions.Length; i++)
            {
                iterator.ProcessTerrainData(i);
                var currentGeometryInstruction = _geometryInstructions[i];
                for (var j = 0; j < iterator.CurrentInstructionSurfaceDistanceReadback.Length; j++)
                {
                    var distance = iterator.CurrentInstructionSurfaceDistanceReadback[j].PackedValues;
                    var isWriting = (distance < 10) & (distance > -10);
                    for (var k = 0; k < 4; k++)
                        if (isWriting[k])
                        {
                            var hash128 = hashPerChunk[k + 4 * j];
                            var hashOfInstruction = currentGeometryInstruction.GeometryInstructionHash;
                            hash128.Append(ref hashOfInstruction);
                            hashPerChunk[k + j * 4] = hash128;
                        }
                }
            }

            var clusterParameters = cluster.Parameters;

            for (var i = 0; i < 512; i++)
            {
                var newInstructionHash = hashPerChunk[i];
                clusterParameters.WriteMask[i] = newInstructionHash != default;

                var chunk = cluster.GetChunk(i);
                var chunkParameters = chunk.Parameters;
                chunkParameters.InstructionsChangedSinceLastFrame =
                    newInstructionHash != chunkParameters.CurrentGeometryInstructionsHash;
                chunkParameters.CurrentGeometryInstructionsHash = newInstructionHash;

                
                //todo placeholder 
                clusterParameters.WriteMask[i] = true;
                chunkParameters.InstructionsChangedSinceLastFrame = true;
                
                
                chunk.Parameters = chunkParameters;
            }

            hashPerChunk.Dispose();
            positions.Dispose();

            cluster.Parameters = clusterParameters;
        }
    }
}