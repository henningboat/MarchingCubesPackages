using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SGeometryInstructionPreparation))]
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
                EntityManager.CreateArchetype(typeof(CGeometryChunk), typeof(PackedDistanceFieldData),
                    typeof(GeometryLayerReference));
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
        }

        private void DestroyLayerAndChildEntities(Entity layerEntity)
        {
            var layerChildren = EntityManager.GetBuffer<CGeometryLayerChild>(layerEntity);
            EntityManager.DestroyEntity(layerChildren.Reinterpret<Entity>().AsNativeArray());

            if (EntityManager.HasComponent<CGeometryLayerGPUBuffer>(layerEntity))
                EntityManager.GetSharedComponentData<CGeometryLayerGPUBuffer>(layerEntity).Dispose();

            if (EntityManager.HasComponent<CLayerMeshData>(layerEntity))
                EntityManager.GetSharedComponentData<CLayerMeshData>(layerEntity).Dispose();

            EntityManager.DestroyEntity(layerEntity);
        }

        private void SpawnLayerAndChildren(GeometryLayerReference reference)
        {
            if (reference.LayerAsset == null) throw new NullReferenceException();

            var layerEntity = EntityManager.CreateEntity(_geometryLayerArchetype);
            EntityManager.SetSharedComponentData(layerEntity, reference);

            EntityManager.SetName(layerEntity, "Layer " + reference.LayerAsset.name);

            var clusterCount = Settings.ClusterCounts.Volume();

            var chunks = SpawnChunks(clusterCount, layerEntity,reference);

            if (reference.LayerAsset.render)
            {
                CGeometryLayerGPUBuffer geometryLayerGPUBuffer = new CGeometryLayerGPUBuffer()
                {
                    Value = new GeometryLayerGPUBuffer(Settings.ClusterCounts)
                };
                AddAllChunksToGPUBuffers(chunks,  geometryLayerGPUBuffer.Value);

                CLayerMeshData renderer = new CLayerMeshData()
                {
                    Value = new LayerMeshData()
                };

                InitializeLayerMeshData(renderer.Value, chunks.Length);

                EntityManager.AddSharedComponentData(layerEntity, geometryLayerGPUBuffer);
                EntityManager.AddSharedComponentData(layerEntity, renderer);
            }

            chunks.Dispose();
        }

        private void InitializeLayerMeshData(LayerMeshData meshBuilder, int chunkCount)
        {
            meshBuilder.ArgsBuffer = new ComputeBuffer(4, 4);
            meshBuilder.ArgsBuffer.SetData(new[] {3, 0, 0, 0});


            meshBuilder.TrianglePositionCountBuffer =
                new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            meshBuilder.TriangleCountPerSubChunk = new ComputeBuffer(chunkCount, 4);

            meshBuilder.ChunksToTriangulate =
                new ComputeBuffer(chunkCount, 4 * 4, ComputeBufferType.Default);
            meshBuilder.IndexBufferCounter = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);

            meshBuilder.TriangulationIndices = new ComputeBuffer(chunkCount * Constants.maxTrianglesPerChunk, 4 * 2);

            meshBuilder.TriangleCountPerSubChunk.SetData(new[] {meshBuilder.TriangleCountPerSubChunk.count});

            var triangleCapacity = chunkCount * Constants.chunkVolume * 5;

            meshBuilder.PropertyBlock = new MaterialPropertyBlock();

            // meshBuilder._triangulationIndices = new ComputeBuffer(triangleCapacity, 4, ComputeBufferType.Structured);
            // meshBuilder._triangleBuffer = new ComputeBuffer(triangleCapacity, 4, ComputeBufferType.Append);
            //
            // meshBuilder._clusterCounts = geometryFieldData.ClusterCounts;
            // meshBuilder._chunkCounts = geometryFieldData.ClusterCounts * Constants.chunkLengthPerCluster;
            // meshBuilder._voxelCounts = geometryFieldData.ClusterCounts * Constants.chunkLengthPerCluster *
            //                            Constants.chunkLength;
            // meshBuilder._propertyBlock = new MaterialPropertyBlock();
        }

        //it would be nicer to spawn the entities with the correct archetype
        private void AddAllChunksToGPUBuffers(NativeArray<Entity> chunks, GeometryLayerGPUBuffer geometryLayerGPUBuffer)
        {
            foreach (var chunkEntity in chunks)
            {
                var chunkGPUBufferIndices = geometryLayerGPUBuffer.RegisterChunkEntity(chunkEntity);
                EntityManager.AddComponentData<CGeometryChunkGPUIndices>(chunkEntity, chunkGPUBufferIndices);
            }
        }

        private NativeArray<Entity> SpawnChunks(int clusterCount, Entity layerEntity,
            GeometryLayerReference geometryLayerReference)
        {
            var clusterEntities = EntityManager.CreateEntity(_entityClusterArchetype, clusterCount, Allocator.Temp);
            var childBuffer = EntityManager.GetBuffer<CGeometryLayerChild>(layerEntity);
            childBuffer.AddRange(clusterEntities.Reinterpret<CGeometryLayerChild>());

            for (var i = 0; i < clusterEntities.Length; i++)
            {
                var position = DistanceFieldGeneration.Utils.IndexToPositionWS(i, Settings.ClusterCounts) *
                               Constants.chunkLength;

                var clusterEntity = clusterEntities[i];

                EntityManager.AddComponentData(clusterEntity, new Parent {Value = layerEntity});

                EntityManager.SetName(clusterEntity, $"Cluster {position.ToString()}");
                EntityManager.SetComponentData(clusterEntity,
                    new CGeometryChunk {PositionWS = position, IndexInIndexMap = i, LayerEntity = layerEntity});
                EntityManager.SetSharedComponentData(clusterEntity, geometryLayerReference);
                var distanceFieldDatas = EntityManager.GetBuffer<PackedDistanceFieldData>(clusterEntity);
                distanceFieldDatas.Length = Constants.chunkVolume / Constants.PackedCapacity;
            }

            return clusterEntities;
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

        public T GetLayer<T>(GeometryLayerReference geometryLayerReference) where T : struct, ISharedComponentData
        {
            return EntityManager.GetSharedComponentData<T>(GetGeometryLayerSingleton(geometryLayerReference));
        }
    }
}