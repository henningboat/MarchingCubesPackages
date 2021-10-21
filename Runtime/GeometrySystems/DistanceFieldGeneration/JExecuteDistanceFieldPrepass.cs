using Code.SIMDMath;
using GeometrySystems.GeometryFieldSetup;
using TerrainChunkEntitySystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace NonECSImplementation
{
    [BurstCompile]
    public struct JExecuteDistanceFieldPrepass:IJobParallelFor
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphData _graph;
       [NativeDisableParallelForRestriction] public NativeArray<bool4> _resultCullingMask;

        public JExecuteDistanceFieldPrepass(GeometryFieldData geometryFieldData, GeometryGraphData graph, NativeArray<bool4> resultCullingMask)
        {
            _graph = graph;
            _resultCullingMask = resultCullingMask;
            _geometryFieldData = geometryFieldData;
        }

        public void Execute(int clusterIndex)
        {
            var cluster = _geometryFieldData.GetCluster(clusterIndex);
            NativeArray<PackedFloat3> positions =
                new NativeArray<PackedFloat3>(Constants.chunksPerCluster / Constants.PackedCapacity, Allocator.Temp);

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
             TerrainInstructionIterator iterator = new TerrainInstructionIterator(positions,_graph.GeometryInstructions,_graph.ValueBuffer);
             iterator.CalculateTerrainData();

             for (int i = 0; i < 128; i++)
             {
                 var distance = iterator._terrainDataBuffer[i].SurfaceDistance.PackedValues;
                 _resultCullingMask[i] = distance < 6 & distance > -6;
             }

             // throw new NotImplementedException();
        }
    }
}