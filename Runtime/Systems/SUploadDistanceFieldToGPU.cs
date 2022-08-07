using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using SIMDMath;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUpdateDistanceField))]
    public partial class SUploadDistanceFieldToGPU : SGeometrySystem
    {
        private SSetupGeometryLayers _setupLayer;

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype(typeof(CGeometryLayerGPUBuffer));
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
            for (int i = 0; i < chunks.Length; i++)
            {
                EntityManager.SetComponentData(chunks[i],
                    new CGeometryChunkGPUIndices() {DistanceFieldBufferOffset = i});
            }
        }

        public override void OnLayerDestroyed(GeometryLayerAssetsReference geometryLayerAssetsReference)
        {
            _setupLayer.GetGeometryLayerSingletonData<CGeometryLayerGPUBuffer>(geometryLayerAssetsReference).Value.Dispose();
        }

        public override List<ComponentType> RequiredComponentsPerChunk => new() {typeof(CGeometryChunkGPUIndices)};

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity, Entity layerEntity,
            CGeometryFieldSettings settings)
        {
            CGeometryLayerGPUBuffer geometryLayerGPUBuffer = new CGeometryLayerGPUBuffer()
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
                gpuBuffers.DistanceFieldBuffer.BeginWrite<float>(0,
                    gpuBuffers.DistanceFieldBuffer.count);
            
            var jobHandle = Entities.WithSharedComponentFilter(geometryLayerReference).ForEach(
                    (in DynamicBuffer<PackedDistanceFieldData> distanceFieldBuffer,
                        in CGeometryChunkGPUIndices chunkGPUIndices, in CGeometryChunk chunk) =>
                    {
                        writableIndexMap[chunk.IndexInIndexMap] = chunkGPUIndices.DistanceFieldBufferOffset;
            
                        var packedChunkLength = Constants.chunkVolume / Constants.PackedCapacity;
                        var baseOffset = Constants.chunkVolume * chunkGPUIndices.DistanceFieldBufferOffset;
                        for (int i = 0; i < Constants.chunkVolume; i++)
                        {
                            writableBuffer[i + baseOffset] = distanceFieldBuffer[i / 8].SurfaceDistance.PackedValues[i % 8];
                        }
                    }).WithBurst().WithNativeDisableParallelForRestriction(writableBuffer)
                .WithNativeDisableParallelForRestriction(writableIndexMap)
                .ScheduleParallel(Dependency);
            jobHandle.Complete();
            
            gpuBuffers.DistanceFieldBuffer.EndWrite<float>(gpuBuffers.DistanceFieldBuffer.count);
            gpuBuffers.IndexMapBuffer.EndWrite<int>(gpuBuffers.IndexMapBuffer.count);
        }
    }
}