using System.Linq;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUpdateDistanceField))]
    public partial class SUploadDistanceFieldToGPU : SystemBase
    {
        private SSetupGeometryLayers _setupLayer;

        protected override void OnCreate()
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
        }

        protected override void OnUpdate()
        {
            foreach (var geometryLayerReference in _setupLayer.ExistingGeometryLayers.Where(reference =>
                         reference.LayerAsset.render))
            {
                var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(geometryLayerReference).Value;

                var writableIndexMap = gpuBuffers.IndexMapBuffer.BeginWrite<int>(0, gpuBuffers.IndexMapBuffer.count);

                var writableBuffer =
                    gpuBuffers.DistanceFieldBuffer.BeginWrite<PackedDistanceFieldData>(0,
                        gpuBuffers.DistanceFieldBuffer.count);

                var jobHandle = Entities.WithSharedComponentFilter(geometryLayerReference).ForEach(
                        (in DynamicBuffer<PackedDistanceFieldData> distanceFieldBuffer,
                            in CGeometryChunkGPUIndices chunkGPUIndices, in CGeometryChunk chunk) =>
                        {
                            writableIndexMap[chunk.IndexInIndexMap] = chunkGPUIndices.DistanceFieldBufferOffset;

                            var packedChunkLength = Constants.chunkVolume / Constants.PackedCapacity;
                            writableBuffer.Slice(chunkGPUIndices.DistanceFieldBufferOffset * packedChunkLength,
                                    packedChunkLength)
                                .CopyFrom(distanceFieldBuffer.AsNativeArray());
                        }).WithBurst().
                    WithNativeDisableParallelForRestriction(writableBuffer).
                    WithNativeDisableParallelForRestriction(writableIndexMap)
                    .ScheduleParallel(Dependency);
                jobHandle.Complete();

                gpuBuffers.DistanceFieldBuffer.EndWrite<PackedDistanceFieldData>(gpuBuffers.DistanceFieldBuffer.count);
                gpuBuffers.IndexMapBuffer.EndWrite<int>(gpuBuffers.IndexMapBuffer.count);
            }
        }
    }
}