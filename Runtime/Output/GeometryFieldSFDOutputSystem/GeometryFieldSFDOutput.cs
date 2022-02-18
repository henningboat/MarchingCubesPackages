using System;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Output.GeometryFieldSFDOutputSystem
{
    public class GeometryFieldSFDOutput : MonoBehaviour, IGeometryFieldReceiver
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public GeometryLayer RequestedLayer()
        {
            throw new NotImplementedException();
        }

        public JobHandle ScheduleJobs(JobHandle dependencies, GeometryFieldData requestedField)
        {
            throw new NotImplementedException();
        }

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            throw new NotImplementedException();
        }

        public void OnJobsFinished(GeometryFieldData geometryFieldData)
        {
            throw new NotImplementedException();
        }
    }
}