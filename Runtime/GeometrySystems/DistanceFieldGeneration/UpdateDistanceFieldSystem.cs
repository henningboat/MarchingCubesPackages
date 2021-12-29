using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
{
    internal class UpdateDistanceFieldSystem
    {
        private GeometryFieldData _geometryFieldData;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }

        public JobHandle Update(JobHandle dependency, NativeArray<GeometryInstruction> graphData)
        {
            var job = new JCalculateDistanceField(_geometryFieldData, graphData);
            dependency = job.Schedule(_geometryFieldData.TotalChunkCount, 1, dependency);
            return dependency;
        }
    }
}