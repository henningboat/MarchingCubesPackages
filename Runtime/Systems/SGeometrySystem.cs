using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using Unity.Collections;
using Unity.Entities;

namespace henningboat.CubeMarching.Runtime.Systems
{
    public abstract partial class SGeometrySystem : SystemBase
    {
        public virtual List<ComponentType> RequiredComponentsPerChunk => new(); 
        private EntityArchetype _archetype;
        private string _systemName;
        protected SSetupGeometryLayers _setupLayer;
        protected abstract EntityArchetype GetArchetype();
        protected abstract bool RunSystemForLayer(GeometryLayerAsset layer);

        protected override void OnCreate()
        {
            _archetype = GetArchetype();
            _systemName = GetType().Name;
            
        }

        protected override void OnStartRunning()
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
        }

        public void OnLayerCreated(GeometryLayerAsset layer, CGeometryFieldSettings settings)
        {
            if (RunSystemForLayer(layer))
            {
                var entity = EntityManager.CreateEntity(_archetype);
                EntityManager.SetName(entity, $"{layer.name}_{_systemName}");
                EntityManager.AddSharedComponentData(entity, new GeometryLayerAssetsReference(layer));
                InitializeLayerHandlerEntity(layer, entity, settings);
            }
        }

        protected override void OnUpdate()
        { 
            foreach (var geometryLayerReference in _setupLayer.ExistingGeometryLayers)
            {
                if (!RunSystemForLayer(geometryLayerReference.LayerAsset))
                {
                    break; 
                }

                UpdateInternal(geometryLayerReference);
            }
        }

        public abstract void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference);

        protected abstract void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity,
            CGeometryFieldSettings settings);

        public virtual void InitializeChunkData(NativeArray<Entity> chunks)
        {
        }

        public virtual void OnLayerDestroyed(GeometryLayerAssetsReference geometryLayerAssetsReference)
        {
        }
    }
}