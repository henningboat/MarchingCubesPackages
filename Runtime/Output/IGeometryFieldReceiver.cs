using System;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public interface IGeometryFieldReceiver : IDisposable
    {
        public GeometryLayer RequestedLayer();

        public JobHandle ScheduleJobs(JobHandle dependencies, GeometryFieldData requestedField,
            NativeList<int> chunkIndexesUpdatedThisFrame);

        public void Initialize(GeometryFieldData geometryFieldData);
        public void OnJobsFinished(GeometryFieldData geometryFieldData);
    }
}