using henningboat.CubeMarching.Runtime.Components;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    public class ComputeShaderHandler
    {
        private ComputeShader _computeShader;

        private int _kernelResetTriangulation;
        private int _getPositionKernel;
        private int _indexBufferKernel;

        public ComputeShaderHandler(ComputeShader computeShader)
        {
            _computeShader = computeShader;


            _kernelResetTriangulation = _computeShader.FindKernel("ResetSubChunkTriangleCount");
            _getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
            _indexBufferKernel = _computeShader.FindKernel("BuildIndexBuffer");
        }

        public void TriangulizeChunks(NativeList<float4> chunksToTriangulate, GeometryLayerGPUBuffer gpuBuffer ,LayerMeshData layerMeshData)
        {
            {
                SetupGeometryLayerProperties(_kernelResetTriangulation, gpuBuffer);
         
                layerMeshData.TrianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});
                layerMeshData.ChunksToTriangulate.SetData(chunksToTriangulate.AsArray());

                _computeShader.SetBuffer(_kernelResetTriangulation, "_ChunksToTriangulate",
                    layerMeshData.ChunksToTriangulate);
                _computeShader.SetBuffer(_kernelResetTriangulation, "_ChunkTriangleCount",
                    layerMeshData.ChunkTriangleCount);
                _computeShader.Dispatch(_kernelResetTriangulation, chunksToTriangulate.Length, 1, 1);
            }

            {
                SetupGeometryLayerProperties(_getPositionKernel, gpuBuffer);
                
                _computeShader.SetBuffer(_getPositionKernel, "_ChunksToTriangulate",
                    layerMeshData.ChunksToTriangulate);
                _computeShader.SetBuffer(_getPositionKernel, "_ChunkTriangleCount",
                    layerMeshData.ChunkTriangleCount);
                _computeShader.SetBuffer(_getPositionKernel, "_TriangleIndices",
                    layerMeshData.TriangulationIndices);

                _computeShader.Dispatch(_getPositionKernel, chunksToTriangulate.Length, 1, 1);
            }
        }

        public void CollectRenderTriangles(NativeList<float4> chunksToRender, GeometryLayerGPUBuffer gpuBuffer,
            LayerMeshData layerMeshData)
        {
            layerMeshData.ChunkBasePositionIndex.SetData(chunksToRender.AsArray());

            SetupGeometryLayerProperties(_indexBufferKernel, gpuBuffer);

            layerMeshData.TrianglesToRenderBuffer.SetData(new uint[layerMeshData.TrianglesToRenderBuffer.count]);

            layerMeshData.IndexBufferCounter.SetData(new uint[] {0, 1, 0, 0});
            _computeShader.SetBuffer(_indexBufferKernel, "_ChunkTriangleCount",
                layerMeshData.ChunkTriangleCount);
            ;
            _computeShader.SetBuffer(_indexBufferKernel, "_ChunksToTriangulate",
                layerMeshData.ChunksToTriangulate);
            _computeShader.SetBuffer(_indexBufferKernel, "_IndexBufferCounter",
                layerMeshData.IndexBufferCounter);
            _computeShader.SetBuffer(_indexBufferKernel, "_TriangleIndices", layerMeshData.TriangulationIndices);
            _computeShader.SetBuffer(_indexBufferKernel, "_TrianglesToRenderBuffer",
                layerMeshData.TrianglesToRenderBuffer);
            _computeShader.SetBuffer(_indexBufferKernel, "_ChunkBasePositionIndex",
                layerMeshData.ChunkBasePositionIndex);

            _computeShader.Dispatch(_indexBufferKernel, chunksToRender.Length, 1, 1);
        }

        private void SetupGeometryLayerProperties(int kernel, GeometryLayerGPUBuffer gpuBuffer)
        {
            _computeShader.SetInt("_ChunkCountsX", gpuBuffer.ChunkCounts.x);
            _computeShader.SetInt("_ChunkCountsY", gpuBuffer.ChunkCounts.y);
            _computeShader.SetInt("_ChunkCountsZ", gpuBuffer.ChunkCounts.z);

            _computeShader.SetBuffer(kernel, "_DistanceField", gpuBuffer.DistanceFieldBuffer);
            _computeShader.SetBuffer(kernel, "_IndexMap", gpuBuffer.IndexMapBuffer);
            _computeShader.SetBuffer(kernel, "_ChunkBasePositions", gpuBuffer.IndexMapBuffer);
        }

        public void SetupGeometryLayerMaterialData(MaterialPropertyBlock block, GeometryLayerGPUBuffer gpuBuffer)
        {
            block.SetInt("_ChunkCountsX", gpuBuffer.ChunkCounts.x);
            block.SetInt("_ChunkCountsY", gpuBuffer.ChunkCounts.y);
            block.SetInt("_ChunkCountsZ", gpuBuffer.ChunkCounts.z);

            block.SetBuffer("_DistanceField", gpuBuffer.DistanceFieldBuffer);
            block.SetBuffer("_IndexMap", gpuBuffer.IndexMapBuffer);
        }
    }
} 