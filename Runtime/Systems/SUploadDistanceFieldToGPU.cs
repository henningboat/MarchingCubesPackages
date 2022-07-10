using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUpdateDistanceField))]
    public partial class SUploadDistanceFieldToGPU : SGeometrySystem
    {
        private SChunkPrepass _prepassSystem;
        private SSetupGeometryLayers _setupLayer;

        public override List<ComponentType> RequiredComponentsPerChunk => new() {typeof(CGeometryChunkGPUIndices)};

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype(typeof(CGeometryLayerGPUBuffer));
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _prepassSystem = World.GetOrCreateSystem<SChunkPrepass>();
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return layer.render;
        }

        protected override void OnCreate()
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
            base.OnCreate();
        }

        public override void InitializeChunkData(NativeArray<Entity> chunks)
        {
            for (var i = 0; i < chunks.Length; i++)
                EntityManager.SetComponentData(chunks[i],
                    new CGeometryChunkGPUIndices {DistanceFieldBufferOffset = i});
        }

        public override void OnLayerDestroyed(GeometryLayerAssetsReference geometryLayerAssetsReference)
        {
            _setupLayer.GetGeometryLayerSingletonData<CGeometryLayerGPUBuffer>(geometryLayerAssetsReference).Value
                .Dispose();
        }

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity,
            CGeometryFieldSettings settings)
        {
            var geometryLayerGPUBuffer = new CGeometryLayerGPUBuffer
            {
                Value = new GeometryLayerGPUBuffer(settings.ClusterCounts)
            };

            EntityManager.SetSharedComponentData(entity, geometryLayerGPUBuffer);
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(geometryLayerReference).Value;
            //
            var writableIndexMap = gpuBuffers.IndexMapBuffer.BeginWrite<int>(0, gpuBuffers.IndexMapBuffer.count);

            var writableBuffer =
                gpuBuffers.DistanceFieldBuffer.BeginWrite<PackedDistanceFieldData>(0,
                    gpuBuffers.DistanceFieldBuffer.count);

            var job = new JUploadDistanceFieldToGPU
            {
                DirtyEntities = _prepassSystem.GetDirtyChunks(geometryLayerReference),
                GetChunkData = GetComponentDataFromEntity<CGeometryChunk>(),
                GPUBuffer = writableBuffer, GetDistanceFieldBuffer = GetBufferFromEntity<PackedDistanceFieldData>(),
                GetChunkGPUIndices = GetComponentDataFromEntity<CGeometryChunkGPUIndices>()
            };

            Dependency = job.Schedule(_setupLayer.TotalChunkCount, 32, Dependency);

            gpuBuffers.DistanceFieldBuffer.EndWrite<PackedDistanceFieldData>(gpuBuffers.DistanceFieldBuffer.count);
            gpuBuffers.IndexMapBuffer.EndWrite<int>(gpuBuffers.IndexMapBuffer.count);
        }


        public struct JUploadDistanceFieldToGPU : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<PackedDistanceFieldData> GPUBuffer;
            [ReadOnly] public NativeList<Entity> DirtyEntities;
            [ReadOnly] public BufferFromEntity<PackedDistanceFieldData> GetDistanceFieldBuffer;
            [ReadOnly] public ComponentDataFromEntity<CGeometryChunk> GetChunkData;
            [ReadOnly] public ComponentDataFromEntity<CGeometryChunkGPUIndices> GetChunkGPUIndices;

            public void Execute(int index)
            {
                if (index >= DirtyEntities.Length)
                    return;

                var entity = DirtyEntities[index];
                var chunkGPUIndices = GetChunkGPUIndices[entity];
                var distanceFieldBuffer = GetDistanceFieldBuffer[entity];

                var packedChunkLength = Constants.chunkVolume / Constants.PackedCapacity;
                GPUBuffer.Slice(chunkGPUIndices.DistanceFieldBufferOffset * packedChunkLength,
                        packedChunkLength)
                    .CopyFrom(distanceFieldBuffer.AsNativeArray());
            }
        }
    }
}