using TerrainChunkEntitySystem;
using Unity.Burst;
using Unity.Jobs;

namespace NonECSImplementation
{
    internal class UpdateDistanceFieldSystem
    {
        private GeometryFieldData _geometryFieldData;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }

        public JobHandle Update(JobHandle dependency, GeometryGraphData graphData)
        {
            var job = new JCalculateDistanceField(_geometryFieldData, graphData);
            dependency = job.Schedule(_geometryFieldData.TotalChunkCount, 1, dependency);
            return dependency;
        }
    }

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

            DistanceFieldResolver.CalculateDistanceFieldForChunk(cluster, chunk, _geometryFieldData, _graph);
        }
    }
}