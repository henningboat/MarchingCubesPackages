using System.Linq;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.Rendering;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUploadDistanceFieldToGPU))]
    public partial class SUpdateChunkMeshes : SystemBase
    {
        private SSetupGeometryLayers _setupLayer;
        private ComputeShader _computeShader;
        private ComputeShaderHandler _computeShaderHandler;
        

        protected override void OnCreate()
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
            _computeShaderHandler = new(DynamicCubeMarchingSettingsHolder.Instance.Compute);

        }

        protected override void OnUpdate()
        {
            foreach (var geometryLayerReference in _setupLayer.ExistingGeometryLayers.Where(reference =>
                         reference.LayerAsset.render))
            {
                UpdateMesh(geometryLayerReference);
            }
        }

        private void UpdateMesh(GeometryLayerReference geometryLayerReference)
        {
            var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(geometryLayerReference).Value;
            var gpuRenderer = _setupLayer.GetLayer<CLayerMeshData>(geometryLayerReference).Value;

            //todo actually filter
            var query = GetEntityQuery(typeof(CGeometryChunkGPUIndices), typeof(GeometryLayerReference));
            query.SetSharedComponentFilter(geometryLayerReference);
            var entitiesToUpdate = query.ToEntityArray(Allocator.Temp);

            var indices = new NativeArray<int>(entitiesToUpdate.Length, Allocator.Temp);
            for (var i = 0; i < entitiesToUpdate.Length; i++)
            {
                indices[i] = EntityManager.GetComponentData<CGeometryChunkGPUIndices>(entitiesToUpdate[i])
                    .DistanceFieldBufferOffset;
            }

            _computeShaderHandler.TriangulizeChunks(indices, gpuBuffers, gpuRenderer);

            indices.Dispose();
            entitiesToUpdate.Dispose();
        }
    }
}