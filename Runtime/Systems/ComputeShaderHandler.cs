using henningboat.CubeMarching.Runtime.Components;
using Unity.Collections;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    public class ComputeShaderHandler
    {
        private ComputeShader _computeShader;

        private int _kernelResetTriangulation;
        private int _getPositionKernel;

        public ComputeShaderHandler(ComputeShader computeShader)
        {
            _computeShader = computeShader;


            _kernelResetTriangulation = _computeShader.FindKernel("ResetSubChunkTriangleCount");
            _getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
        }

        public void TriangulizeChunks(NativeArray<int> chunksToTriangulate, GeometryLayerGPUBuffer gpuBuffer ,LayerMeshData layerMeshData)
        {
            SetupGeometryLayerProperties(_getPositionKernel, gpuBuffer);
            
            {
                layerMeshData.TrianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});
                layerMeshData.ChunksToTriangulize.SetData(chunksToTriangulate);

                var resetTriangleCountKernel = _computeShader.FindKernel("ResetSubChunkTriangleCount");
                _computeShader.SetBuffer(resetTriangleCountKernel, "_ChunksToTriangulate",
                    layerMeshData.ChunksToTriangulize);
                _computeShader.SetBuffer(resetTriangleCountKernel, "_TriangleCountPerSubChunk",
                    layerMeshData.TriangleCountPerSubChunk);
                _computeShader.Dispatch(resetTriangleCountKernel, chunksToTriangulate.Length, 1, 1);
            }
            
            
            _computeShader.Dispatch(_getPositionKernel, chunksToTriangulate.Length, 1, 1);
        }

        private void SetupGeometryLayerProperties(int kernel, GeometryLayerGPUBuffer gpuBuffer)
        {
            _computeShader.SetInt("_ChunkCountsX", gpuBuffer.ChunkCounts.x);
            _computeShader.SetInt("_ChunkCountsY", gpuBuffer.ChunkCounts.y);
            _computeShader.SetInt("_ChunkCountsZ", gpuBuffer.ChunkCounts.z);

            _computeShader.SetBuffer(kernel, "_ChunkCountsZ", gpuBuffer.ChunkCounts.z);

        }
    }
}