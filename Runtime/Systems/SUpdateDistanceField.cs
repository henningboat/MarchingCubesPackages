using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SChunkPrepass))]
    public partial class SUpdateDistanceField : SGeometrySystem
    {
        private EntityQuery _clusterQuery;
        private SChunkPrepass _prepassSystem;

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype();
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return true;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _clusterQuery = GetEntityQuery(typeof(CGeometryChunk), typeof(PackedDistanceFieldData));
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _prepassSystem = World.GetOrCreateSystem<SChunkPrepass>();
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var instructionsHolderEntity =
                _setupLayer.GetGeometryLayerSingleton<CGeometryLayerInstance>(geometryLayerReference);
            var dirtyEntities = _prepassSystem.GetDirtyChunks(geometryLayerReference);
            var job = new JUpdateDistanceField
            {
                DirtyEntities = dirtyEntities,
                Instructions = EntityManager.GetBuffer<GeometryInstruction>(instructionsHolderEntity),
                GetChunkData = GetComponentDataFromEntity<CGeometryChunk>(),
                ReadbackHandler = new ReadbackHandler(this),
            };
            Dependency = job.Schedule(_setupLayer.TotalChunkCount, 1, Dependency);
        }

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity, Entity layerEntity,
            CGeometryFieldSettings settings)
        {
        }
    }
}