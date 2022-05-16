using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [UpdateAfter(typeof(SSetupGeometryLayers))]
    public partial class SUpdateDistanceField : SystemBase
    {
        private EntityQuery _clusterQuery;

        protected override void OnCreate()
        {
            _clusterQuery = GetEntityQuery(typeof(CGeometryCluster), typeof(PackedDistanceFieldData));
        }

        protected override void OnUpdate()
        {
            var instructionsFromEntity = GetBufferFromEntity<GeometryInstruction>(true);

            var job = new JUpdateDistanceFieldInCluster
            {
                GetInstructionsFromEntity = instructionsFromEntity
            };
            Dependency = job.ScheduleParallel(_clusterQuery, Dependency);
        }
    }
}