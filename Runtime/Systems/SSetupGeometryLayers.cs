using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    public partial class SSetupGeometryLayers : SystemBase
    {
        private EntityArchetype _entityClusterArchetype;

        private static readonly CGeometryFieldSettings Settings = new CGeometryFieldSettings
            {ClusterCounts = new int3(2, 2, 2)};

        private readonly List<GeometryLayerReference> _geometryLayerReferencesList = new List<GeometryLayerReference>();
        private EntityArchetype _geometryLayerArchetype;

        private List<GeometryLayerReference> _existingGeometryLayers = new List<GeometryLayerReference>();

        public IReadOnlyList<GeometryLayerReference> ExistingGeometryLayers => _existingGeometryLayers;

        protected override void OnCreate()
        {
            _entityClusterArchetype =
                EntityManager.CreateArchetype(typeof(CGeometryCluster), typeof(PackedDistanceFieldData));
            _geometryLayerArchetype =
                EntityManager.CreateArchetype(typeof(GeometryInstruction), typeof(CGeometryLayerTag),
                    typeof(CGeometryLayerChild), typeof(GeometryLayerReference));
        }

        public bool TryGetGeometryLayerSingleton(GeometryLayerReference geometryLayerReference, out Entity entity)
        {
            var layerQuery = EntityManager.CreateEntityQuery(typeof(GeometryInstruction),
                typeof(GeometryLayerReference), typeof(CGeometryLayerTag));
            layerQuery.AddSharedComponentFilter(geometryLayerReference);
            if (layerQuery.IsEmpty)
            {
                entity = Entity.Null;
                return false;
            }

            entity = layerQuery.GetSingletonEntity();
            return true;
        }

        protected override void OnUpdate()
        {
            _geometryLayerReferencesList.Clear();
            _existingGeometryLayers.Clear();
            EntityManager.GetAllUniqueSharedComponentData(_geometryLayerReferencesList);

            for (var i = 0; i < _geometryLayerReferencesList.Count; i++)
            {
                var geometryLayerReference = _geometryLayerReferencesList[i];

                if (geometryLayerReference.LayerAsset == null) continue;

                var newQuery =
                    EntityManager.CreateEntityQuery(typeof(GeometryInstruction), typeof(GeometryLayerReference),
                        typeof(CGeometryInstructionSourceTag));
                newQuery.AddSharedComponentFilter(geometryLayerReference);
                var queryIsEmpty = newQuery.IsEmpty;

                var layerExists = TryGetGeometryLayerSingleton(geometryLayerReference, out var layerEntity);

                if (queryIsEmpty && layerExists)
                {
                    DestroyLayerAndChildEntities(layerEntity);
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


            // foreach (var existingLayer in _layerEntityPerLayer)
            // {
            //     if (!_geometryLayerReferencesList.Contains(existingLayer.Key))
            //     {
            //         DestroyLayerAndChildEntities(existingLayer);
            //         break;
            //     }
            // }
            //
            // foreach (var reference in _geometryLayerReferencesList)
            // {
            //     if (reference.LayerAsset == null) continue;
            //
            //     if (!_layerEntityPerLayer.ContainsKey(reference))
            //     {
            //         SpawnLayerAndChildren(reference);
            //         Debug.Log("no reference for " + reference.LayerAsset);
            //     }
            // }
        }

        private void DestroyLayerAndChildEntities(Entity layerEntity)
        {
            var layerChildren = EntityManager.GetBuffer<CGeometryLayerChild>(layerEntity);
            EntityManager.DestroyEntity(layerChildren.Reinterpret<Entity>().AsNativeArray());
            EntityManager.DestroyEntity(layerEntity);
        }

        private void SpawnLayerAndChildren(GeometryLayerReference reference)
        {
            if (reference.LayerAsset == null) throw new NullReferenceException();

            var layerEntity = EntityManager.CreateEntity(_geometryLayerArchetype);
            EntityManager.SetSharedComponentData(layerEntity, reference);

            EntityManager.SetName(layerEntity, "Layer " + reference.LayerAsset.name);

            var clusterCount = Settings.ClusterCounts.Volume();

            var clusterEntities = EntityManager.CreateEntity(_entityClusterArchetype, clusterCount, Allocator.Temp);
            var childBuffer = EntityManager.GetBuffer<CGeometryLayerChild>(layerEntity);
            childBuffer.AddRange(clusterEntities.Reinterpret<CGeometryLayerChild>());

            for (var i = 0; i < clusterEntities.Length; i++)
            {
                var position = DistanceFieldGeneration.Utils.IndexToPositionWS(i, Settings.ClusterCounts) *
                               Constants.chunkLength;

                var clusterEntity = clusterEntities[i];
                EntityManager.SetName(clusterEntity, $"Cluster {position.ToString()}");
                EntityManager.SetComponentData(clusterEntity,
                    new CGeometryCluster {PositionWS = position, LayerEntity = layerEntity});
                var distanceFieldDatas = EntityManager.GetBuffer<PackedDistanceFieldData>(clusterEntity);
                distanceFieldDatas.Length = Constants.chunkVolume;
            }
        }

        public Entity GetGeometryLayerSingleton(GeometryLayerReference layerReference)
        {
            if (TryGetGeometryLayerSingleton(layerReference, out Entity entity))
            {
                return entity;
            }

            throw new ArgumentOutOfRangeException(nameof(layerReference), layerReference.ToString(),
                "Layer singleton does not exist");
        }
    }
}