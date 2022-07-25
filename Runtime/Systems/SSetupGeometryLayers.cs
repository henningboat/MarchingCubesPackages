using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways] 
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SGeometryInstructionPreparation))]
    public partial class SSetupGeometryLayers : SystemBase 
    {
        public  int TotalChunkCount { get; private set; }
        private EntityArchetype _entityClusterArchetype;

        private static readonly CGeometryFieldSettings Settings = new() {ClusterCounts = new int3(12, 4, 2)};

        private List<GeometryLayerAssetsReference> _geometryLayerReferencesList = new();
        private EntityArchetype _geometryLayerArchetype;

        private List<GeometryLayerAssetsReference> _existingGeometryLayers = new();
        private SGeometrySystem[] _supportSystems;

        private NativeList<CGeometryLayerListElement> _geometryLayerList;
        private Entity _layerListEntity;

        public DynamicBuffer<CGeometryLayerListElement> GeometryLayerList =>
            EntityManager.GetBuffer<CGeometryLayerListElement>(_layerListEntity, true);

        public IReadOnlyList<GeometryLayerAssetsReference> ExistingGeometryLayers => _existingGeometryLayers;

        protected override void OnCreate()
        {
            _supportSystems = TypeCache.GetTypesDerivedFrom<SGeometrySystem>()
                .Select(t => World.GetOrCreateSystem(t)).Cast<SGeometrySystem>().ToArray();

            _geometryLayerArchetype =
                EntityManager.CreateArchetype(typeof(GeometryInstruction), typeof(CGeometryLayerTag),
                    typeof(CGeometryLayerChild), typeof(GeometryLayerAssetsReference));

            _geometryLayerList = new NativeList<CGeometryLayerListElement>(Allocator.Persistent);
            
            List<ComponentType> typesOfEntityClusterArchetype = new List<ComponentType>()
            {
                typeof(CGeometryChunk), 
                typeof(PackedDistanceFieldData),
                typeof(GeometryLayerAssetsReference),
                typeof(CGeometryChunkState)
            };
            
            foreach (SGeometrySystem supportSystem in _supportSystems)
            {
                typesOfEntityClusterArchetype.AddRange(supportSystem.RequiredComponentsPerChunk);
            }

            TotalChunkCount = Settings.ClusterCounts.Volume();

            _entityClusterArchetype =
                EntityManager.CreateArchetype(typesOfEntityClusterArchetype.ToArray());

            _layerListEntity = EntityManager.CreateEntity(typeof(CGeometryLayerListElement));
        }

        public bool TryGetGeometryLayerSingleton<T>(GeometryLayerAssetsReference geometryLayerAssetsReference, out Entity entity) where T : struct
        {
            var layerQuery = EntityManager.CreateEntityQuery(typeof(T),
                typeof(GeometryLayerAssetsReference));
            layerQuery.AddSharedComponentFilter(geometryLayerAssetsReference);
            if (layerQuery.IsEmpty)
            {
                entity = Entity.Null;
                return false;
            }

            entity = layerQuery.GetSingletonEntity();
            return true;
        }

        public T GetGeometryLayerSingletonData<T>(GeometryLayerAssetsReference geometryLayerAssetsReference)
            where T : struct, ISharedComponentData
        {
            if (TryGetGeometryLayerSingletonData<T>(geometryLayerAssetsReference, out var result)) return result;

            throw new ArgumentOutOfRangeException(nameof(geometryLayerAssetsReference),
                geometryLayerAssetsReference.ToString(),
                "Layer singleton does not exist");
        }

        public bool TryGetGeometryLayerSingletonData<T>(GeometryLayerAssetsReference geometryLayerAssetsReference,
            out T result) where T : struct, ISharedComponentData
        {
            if (TryGetGeometryLayerSingleton<T>(geometryLayerAssetsReference, out var e))
            {
                result = EntityManager.GetSharedComponentData<T>(e);
                return true;
            }

            result = default;
            return false;
        }


        protected override void OnUpdate()
        {
            _geometryLayerReferencesList.Clear();
            _existingGeometryLayers.Clear();
            EntityManager.GetAllUniqueSharedComponentData(_geometryLayerReferencesList);
            _geometryLayerReferencesList= _geometryLayerReferencesList.OrderBy(reference => reference.LayerAsset != null ? reference.LayerAsset.order : int.MinValue).ToList();

            for (var i = 0; i < _geometryLayerReferencesList.Count; i++)
            {
                var geometryLayerReference = _geometryLayerReferencesList[i];
            
                if (geometryLayerReference.LayerAsset == null) continue;
            
                var newQuery =
                    EntityManager.CreateEntityQuery(typeof(GeometryInstruction), typeof(GeometryLayerAssetsReference),
                        typeof(CGeometryInstructionSourceTag));
                newQuery.AddSharedComponentFilter(geometryLayerReference);
                var queryIsEmpty = newQuery.IsEmpty;
            
                var layerExists = TryGetGeometryLayerSingleton<CGeometryLayerTag>(geometryLayerReference, out var layerEntity);
            
                if (queryIsEmpty && layerExists)
                {
                    DestroyLayerAndChildEntities(geometryLayerReference);
                }
                
                if (!queryIsEmpty && !layerExists)
                {
                    SpawnLayerAndChildren(geometryLayerReference);
                }
                
                if (!queryIsEmpty)
                {
                    _existingGeometryLayers.Add(geometryLayerReference);
                }
            }
        }

        private void DestroyLayerAndChildEntities(GeometryLayerAssetsReference geometryLayerAssetsReference)
        {
            foreach (var supportSystem in _supportSystems) supportSystem.OnLayerDestroyed(geometryLayerAssetsReference);

            var queryForLayer = EntityManager.CreateEntityQuery(typeof(GeometryLayerAssetsReference));
            queryForLayer.AddSharedComponentFilter(geometryLayerAssetsReference);
            EntityManager.DestroyEntity(queryForLayer);


            var layerList = EntityManager.GetBuffer<CGeometryLayerListElement>(_layerListEntity);
            for (var i = 0; i < layerList.Length; i++)
                if (layerList[i].LayerID == geometryLayerAssetsReference.LayerAsset.geometryLayerID)
                {
                    layerList.RemoveAt(i);
                    i--;
                }
        }

        private void SpawnLayerAndChildren(GeometryLayerAssetsReference assetsReference)
        {
            if (assetsReference.LayerAsset == null) throw new NullReferenceException();

            var layerEntity = EntityManager.CreateEntity(_geometryLayerArchetype);
            EntityManager.SetSharedComponentData(layerEntity, assetsReference);

            EntityManager.SetName(layerEntity, "Layer " + assetsReference.LayerAsset.name);

             var clusterCount = Settings.ClusterCounts.Volume();
            
             var chunks = SpawnChunks(clusterCount, layerEntity,assetsReference);

             foreach (var supportSystem in _supportSystems)
             {
                 supportSystem.OnLayerCreated(assetsReference.LayerAsset,Settings);
             }
            
             EntityQueryDesc ChunksWithoutComponentADesc
                 = new EntityQueryDesc()
                 {
                     None = new []{ComponentType.ChunkComponent<CGeometryLayerReference>() },
                     All = new ComponentType[]{ComponentType.ReadOnly<GeometryLayerAssetsReference>(), }
                 };
             EntityQuery ChunksWithoutChunkComponentA
                 = GetEntityQuery(ChunksWithoutComponentADesc);
            
             EntityManager.AddChunkComponentData<CGeometryLayerReference>(
                 ChunksWithoutChunkComponentA,
                 new CGeometryLayerReference() {LayerEntity = layerEntity});

            chunks.Dispose();

            EntityManager.GetBuffer<CGeometryLayerListElement>(_layerListEntity, false).Add(
                new CGeometryLayerListElement()
                    {InstructionListHandler = layerEntity, LayerID = assetsReference.LayerAsset.geometryLayerID});
        }

        private NativeArray<Entity> SpawnChunks(int clusterCount, Entity layerEntity,
            GeometryLayerAssetsReference geometryLayerAssetsReference)
        {
            var clusterEntities = EntityManager.CreateEntity(_entityClusterArchetype, clusterCount, Allocator.Temp);

            var childBuffer = EntityManager.GetBuffer<CGeometryLayerChild>(layerEntity);
            childBuffer.AddRange(clusterEntities.Reinterpret<CGeometryLayerChild>());

            for (var i = 0; i < clusterEntities.Length; i++)
            {
                var position = DistanceFieldGeneration.Utils.IndexToPositionWS(i, Settings.ClusterCounts) *
                               Constants.chunkLength;

                Entity chunkEntity = clusterEntities[i];

                EntityManager.AddComponentData(chunkEntity, new Parent {Value = layerEntity});

                EntityManager.SetName(chunkEntity, $"Cluster {position.ToString()}");
                EntityManager.SetComponentData(chunkEntity,
                    new CGeometryChunk {PositionWS = position, IndexInIndexMap = i});
                EntityManager.SetSharedComponentData(chunkEntity, geometryLayerAssetsReference);
                var distanceFieldDatas = EntityManager.GetBuffer<PackedDistanceFieldData>(chunkEntity);
                distanceFieldDatas.Length = Constants.chunkVolume / Constants.PackedCapacity;

            }
            var componentTypes = _entityClusterArchetype.GetComponentTypes(Allocator.Temp);
            var query = GetEntityQuery(componentTypes);
            NativeArray<ArchetypeChunk> chunks = query.CreateArchetypeChunkArray(Allocator.Temp);

            foreach (var supportSystem in _supportSystems) supportSystem.InitializeChunkData(clusterEntities);

            chunks.Dispose();
                
            return clusterEntities;
        }
 
        public Entity GetGeometryLayerSingleton<T>(GeometryLayerAssetsReference layerAssetsReference) where T : struct
        {
            if (TryGetGeometryLayerSingleton<T>(layerAssetsReference, out Entity entity))
            {
                return entity;
            }

            throw new ArgumentOutOfRangeException(nameof(layerAssetsReference), layerAssetsReference.ToString(),
                "Layer singleton does not exist");
        }

        public T GetLayer<T>(GeometryLayerAssetsReference geometryLayerAssetsReference) where T : struct, ISharedComponentData
        {
            return EntityManager.GetSharedComponentData<T>(GetGeometryLayerSingleton<T>(geometryLayerAssetsReference));
        }
    }
} 