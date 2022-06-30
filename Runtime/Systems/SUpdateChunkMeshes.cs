using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUploadDistanceFieldToGPU))]
    public partial class SUpdateChunkMeshes : SGeometrySystem
    {
        private SSetupGeometryLayers _setupLayer;
        private ComputeShader _computeShader;
        private ComputeShaderHandler _computeShaderHandler;


        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype(typeof(CLayerMeshData));
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return layer.render;
        }

        protected override void OnCreate()
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
            _computeShaderHandler = new(DynamicCubeMarchingSettingsHolder.Instance.Compute);
            base.OnCreate();
        }
        
        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity,
            CGeometryFieldSettings settings)
        {
            var meshBuilder = new LayerMeshData();
            meshBuilder.ArgsBuffer = new ComputeBuffer(4, 4);
            meshBuilder.ArgsBuffer.SetData(new[] {3, 0, 0, 0});

            var chunkCount = settings.ClusterCounts.Volume();
            var layerDebugName = layer.name;

            meshBuilder.TrianglePositionCountBuffer =
                new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            meshBuilder.ChunkTriangleCount = new ComputeBuffer(chunkCount, 4);

            meshBuilder.ChunksToTriangulate =
                new ComputeBuffer(chunkCount, 4 * 4, ComputeBufferType.Default);

            meshBuilder.ChunkBasePositionIndex =
                new ComputeBuffer(chunkCount, 4 * 4, ComputeBufferType.Default);

            meshBuilder.IndexBufferCounter = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);

            meshBuilder.TriangulationIndices = new ComputeBuffer(chunkCount * Constants.maxTrianglesPerChunk, 4);

            meshBuilder.ChunkTriangleCount.SetData(new[] {meshBuilder.ChunkTriangleCount.count});

            var triangleCapacity = chunkCount * Constants.chunkVolume * 5;

            meshBuilder.PropertyBlock = new MaterialPropertyBlock();

            meshBuilder.TriangleBuffer = new ComputeBuffer(triangleCapacity, 4)
                {name = layerDebugName + "TriangleBuffer"};
            meshBuilder.TrianglesToRenderBuffer = new ComputeBuffer(triangleCapacity, 4)
                {name = layerDebugName + "TrianglesToRenderBuffer"};
      
            EntityManager.SetSharedComponentData(entity, new CLayerMeshData {Value = meshBuilder});
        }

        public override void UpdateInternal(GeometryLayerAssetsReference layerReference)
        {
            var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(layerReference).Value;
            var gpuRenderer = _setupLayer.GetLayer<CLayerMeshData>(layerReference).Value;

            //todo actually filter
            var query = GetEntityQuery(typeof(CGeometryChunkGPUIndices), typeof(GeometryLayerAssetsReference));
            query.SetSharedComponentFilter(layerReference);
            var entitiesToUpdate = query.ToEntityArray(Allocator.Temp);

            var chunksToTriangulate = new NativeList<float4>(Allocator.Temp);

            for (var i = 0; i < entitiesToUpdate.Length; i++)
            {
                chunksToTriangulate.Add(
                    EntityManager.GetComponentData<CGeometryChunk>(entitiesToUpdate[i]).PositionWS.xyzz);
            }

            _computeShaderHandler.TriangulizeChunks(chunksToTriangulate, gpuBuffers, gpuRenderer);


            chunksToTriangulate.Dispose();
            entitiesToUpdate.Dispose();
        }
    }
}