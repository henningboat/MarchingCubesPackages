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
            {ClusterCounts = new int3(8, 8, 8)};

        private readonly List<GeometryLayerAssetsReference> _geometryLayerReferencesList = new List<GeometryLayerAssetsReference>();
        private EntityArchetype _geometryLayerArchetype;

        private List<GeometryLayerAssetsReference> _existingGeometryLayers = new List<GeometryLayerAssetsReference>();

        public IReadOnlyList<GeometryLayerAssetsReference> ExistingGeometryLayers => _existingGeometryLayers;

        protected override void OnCreate()
        {
            _entityClusterArchetype =
                EntityManager.CreateArchetype(typeof(CGeometryChunk), typeof(PackedDistanceFieldData),
                    typeof(GeometryLayerAssetsReference),typeof(CGeometryChunkGPUIndices),ComponentType.ChunkComponent<CGeometryLayerReference>());
            _geometryLayerArchetype =
                EntityManager.CreateArchetype(typeof(GeometryInstruction), typeof(CGeometryLayerTag),
                    typeof(CGeometryLayerChild), typeof(GeometryLayerAssetsReference));
        }

        public bool TryGetGeometryLayerSingleton(GeometryLayerAssetsReference geometryLayerAssetsReference, out Entity entity)
        {
            var layerQuery = EntityManager.CreateEntityQuery(typeof(GeometryInstruction),
                typeof(GeometryLayerAssetsReference), typeof(CGeometryLayerTag));
            layerQuery.AddSharedComponentFilter(geometryLayerAssetsReference);
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
                    EntityManager.CreateEntityQuery(typeof(GeometryInstruction), typeof(GeometryLayerAssetsReference),
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

        private void SpawnLayerAndChildren(GeometryLayerAssetsReference assetsReference)
        {
            if (assetsReference.LayerAsset == null) throw new NullReferenceException();

            var layerEntity = EntityManager.CreateEntity(_geometryLayerArchetype);
            EntityManager.SetSharedComponentData(layerEntity, assetsReference);

            EntityManager.SetName(layerEntity, "Layer " + assetsReference.LayerAsset.name);

            var clusterCount = Settings.ClusterCounts.Volume();

            var chunks = SpawnChunks(clusterCount, layerEntity,assetsReference);

            if (assetsReference.LayerAsset.render)
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

            meshBuilder.ChunkTriangleCount = new ComputeBuffer(chunkCount, 4);

            meshBuilder.ChunksToTriangulate =
                new ComputeBuffer(chunkCount, 4 * 4, ComputeBufferType.Default);
            
            meshBuilder.ChunkBasePositionIndex =
                new ComputeBuffer(chunkCount, 4 * 4, ComputeBufferType.Default);
            
            meshBuilder.IndexBufferCounter = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);

            meshBuilder.TriangulationIndices = new ComputeBuffer(chunkCount * Constants.maxTrianglesPerChunk, 4 );

            meshBuilder.ChunkTriangleCount.SetData(new[] {meshBuilder.ChunkTriangleCount.count});

            var triangleCapacity = chunkCount * Constants.chunkVolume * 5;

            meshBuilder.PropertyBlock = new MaterialPropertyBlock();

            // meshBuilder._triangulationIndices = new ComputeBuffer(triangleCapacity, 4, ComputeBufferType.Structured);
            meshBuilder.TriangleBuffer = new ComputeBuffer(triangleCapacity, 4);
            meshBuilder.TrianglesToRenderBuffer = new ComputeBuffer(triangleCapacity, 4);
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
                EntityManager.SetComponentData<CGeometryChunkGPUIndices>(chunkEntity, chunkGPUBufferIndices);
            }
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

            for (int i = 0; i < chunks.Length; i++)
            {
                EntityManager.SetChunkComponentData(chunks[i],
                    new CGeometryLayerReference() {LayerEntity = layerEntity});
            }

            //
            // var componentTypes = _entityClusterArchetype.GetComponentTypes(Allocator.Temp);
            // var query = GetEntityQuery(componentTypes);
            // query.AddSharedComponentFilter(geometryLayerReference);
            //
            // EntityManager.AddChunkComponentData<CGeometryLayerReference>(query,
            //     new CGeometryLayerReference() {LayerEntity = layerEntity});
            // componentTypes.Dispose();
            chunks.Dispose();
                
            return clusterEntities;
        }

        public Entity GetGeometryLayerSingleton(GeometryLayerAssetsReference layerAssetsReference)
        {
            if (TryGetGeometryLayerSingleton(layerAssetsReference, out Entity entity))
            {
                return entity;
            }

            throw new ArgumentOutOfRangeException(nameof(layerAssetsReference), layerAssetsReference.ToString(),
                "Layer singleton does not exist");
        }

        public T GetLayer<T>(GeometryLayerAssetsReference geometryLayerAssetsReference) where T : struct, ISharedComponentData
        {
            return EntityManager.GetSharedComponentData<T>(GetGeometryLayerSingleton(geometryLayerAssetsReference));
        }
    }
}