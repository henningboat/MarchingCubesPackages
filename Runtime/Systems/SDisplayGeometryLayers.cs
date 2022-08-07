using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SUpdateChunkMeshes))]
    public partial class SDisplayGeometryLayers : SGeometrySystem
    {
        private SSetupGeometryLayers _setupLayer;
        private ComputeShaderHandler _computeShaderHandler;
        private static readonly int ClusterPositionWSID = Shader.PropertyToID("_ClusterPositionWS");
        private static readonly int TriangleIndecesID = Shader.PropertyToID("_TriangleIndeces");
        private SChunkPrepass _prepassSystem;

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype();
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return layer.render;
        }

        protected override void OnCreate() 
        {
            _setupLayer = World.GetOrCreateSystem<SSetupGeometryLayers>();
            _prepassSystem = World.GetOrCreateSystem<SChunkPrepass>();
            _computeShaderHandler = new(DynamicCubeMarchingSettingsHolder.Instance.Compute);
            base.OnCreate();
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var entitiesToDraw = _prepassSystem.GetChunksToDraw(geometryLayerReference);
            if (entitiesToDraw.Length == 0)
            {
                return;
            }

            var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(geometryLayerReference).Value;
            var gpuRenderer = _setupLayer.GetLayer<CLayerMeshData>(geometryLayerReference).Value;


            var chunkBasePositions = new NativeList<float4>(Allocator.Temp);

            for (var i = 0; i < entitiesToDraw.Length; i++)
            {
                chunkBasePositions.Add(
                    EntityManager.GetComponentData<CGeometryChunk>(entitiesToDraw[i]).PositionWS.xyzz);
            }

            var propertyBlock = gpuRenderer.PropertyBlock;

            _computeShaderHandler.CollectRenderTriangles(chunkBasePositions, gpuBuffers, gpuRenderer);
            _computeShaderHandler.SetupGeometryLayerMaterialData(propertyBlock, gpuBuffers);

            propertyBlock.SetVector(ClusterPositionWSID, (Vector3) (float3) 0);
            propertyBlock.SetBuffer("_ChunkPositions", gpuRenderer.ChunksToTriangulate);
            propertyBlock.SetBuffer(TriangleIndecesID, gpuRenderer.TrianglesToRenderBuffer);

            foreach (var material in geometryLayerReference.LayerAsset.materials)
            {
                if (material != null)
                    Graphics.DrawProceduralIndirect(material,
                        new Bounds(Vector3.zero, Vector3.one * 10000),
                        MeshTopology.Triangles, gpuRenderer.IndexBufferCounter, 0, null, propertyBlock);
            }
        }

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity, Entity layerEntity,
            CGeometryFieldSettings settings)
        {
        }
    }
}