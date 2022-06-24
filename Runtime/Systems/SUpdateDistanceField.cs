using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SSetupGeometryLayers))]
    public partial class SUpdateDistanceField : SystemBase
    {
        private EntityQuery _clusterQuery;

        protected override void OnCreate()
        {
            _clusterQuery = GetEntityQuery(typeof(CGeometryChunk), typeof(PackedDistanceFieldData));
        }

        protected override void OnUpdate()
        {
            var instructionsFromEntity = GetBufferFromEntity<GeometryInstruction>(true);

            var job = new JUpdateDistanceField
            {
                GetInstructionsFromEntity = instructionsFromEntity,
                GeometryChunkTypeHandle = EntityManager.GetComponentTypeHandle<CGeometryChunk>(true),
                CGeometryLayerReferenceHandle = EntityManager.GetComponentTypeHandle<CGeometryLayerReference>(true),
                PackedDistanceFieldDataHandle = EntityManager.GetBufferTypeHandle<PackedDistanceFieldData>(false)
            };
            Dependency = job.ScheduleParallel(_clusterQuery, Dependency);
        }
    }
}