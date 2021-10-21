using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using Unity.Jobs;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
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