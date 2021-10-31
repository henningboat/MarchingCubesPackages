using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Burst;
using Unity.Jobs;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    internal struct JCalculateDistanceField : IJobParallelFor
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphData _graph;

        public JCalculateDistanceField(GeometryFieldData geometryFieldData, GeometryGraphData graph)
        {
            _graph = graph;
            _geometryFieldData = geometryFieldData;
        }

        public void Execute(int index)
        {
            var clusterIndex = index / Constants.chunksPerCluster;
            var chunkIndexInCluster = index % Constants.chunksPerCluster;

            var cluster = _geometryFieldData.GetCluster(clusterIndex);
            var chunk = cluster.GetChunk(chunkIndexInCluster);

            DistanceFieldResolver.CalculateDistanceFieldForChunk(cluster, chunk, _graph);
        }
    }
}