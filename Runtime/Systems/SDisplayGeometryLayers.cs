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
            _computeShaderHandler = new(DynamicCubeMarchingSettingsHolder.Instance.Compute);
            base.OnCreate();
        }
        
        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        { 
            var gpuBuffers = _setupLayer.GetLayer<CGeometryLayerGPUBuffer>(geometryLayerReference).Value;
            var gpuRenderer = _setupLayer.GetLayer<CLayerMeshData>(geometryLayerReference).Value;

              
            //todo actually filter which chunks contain data
            var query = GetEntityQuery(typeof(CGeometryChunkGPUIndices), typeof(GeometryLayerAssetsReference));
            query.SetSharedComponentFilter(geometryLayerReference);
            var entitiesToUpdate = query.ToEntityArray(Allocator.Temp);

            var chunkBasePositions = new NativeList<float4>(Allocator.Temp);

            for (var i = 0; i < entitiesToUpdate.Length; i++)
            {
                chunkBasePositions.Add(
                    EntityManager.GetComponentData<CGeometryChunk>(entitiesToUpdate[i]).PositionWS.xyzz);
            }
                
            var propertyBlock = gpuRenderer.PropertyBlock;

            _computeShaderHandler.CollectRenderTriangles(chunkBasePositions, gpuBuffers, gpuRenderer);
            _computeShaderHandler.SetupGeometryLayerMaterialData(propertyBlock, gpuBuffers);
                
            propertyBlock.SetVector(ClusterPositionWSID, (Vector3) (float3) 0);
            propertyBlock.SetBuffer("_ChunkPositions", gpuRenderer.ChunksToTriangulate);
            propertyBlock.SetBuffer(TriangleIndecesID, gpuRenderer.TrianglesToRenderBuffer);
                
            Graphics.DrawProceduralIndirect(geometryLayerReference.LayerAsset.material,
                new Bounds(Vector3.zero, Vector3.one * 10000),
                MeshTopology.Triangles, gpuRenderer.IndexBufferCounter, 0, null, propertyBlock);

        }

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity, CGeometryFieldSettings settings)
        {
        }
    }
}