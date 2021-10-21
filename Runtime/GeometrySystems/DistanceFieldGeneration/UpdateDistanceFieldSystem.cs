using GeometrySystems.GeometryFieldSetup;
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
}