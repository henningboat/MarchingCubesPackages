using System;
using System.Linq;
using System.Security.Cryptography;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.Rendering
{
    internal struct ClusterMeshGPUBuffers
    {
        private ComputeShader _computeShader;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _trianglePositionBuffer;
        private ComputeBuffer _trianglePositionCountBuffer;
        private ComputeBuffer _chunksToTriangulize;
        private ComputeBuffer _triangleCountPerSubChunk;
        private ComputeBuffer _indexBufferCounter;
        private ComputeBuffer _vertexBuffer;
        private ComputeBuffer _indexBuffer;
        private int3 _clusterCounts;
        private int3 _chunkCounts;
        private ComputeBuffer _triangulationIndices;
        private MaterialPropertyBlock _propertyBlock;

        public static ClusterMeshGPUBuffers CreateGPUData(GeometryFieldData geometryFieldData)
        {
            ClusterMeshGPUBuffers result = default;
            result._computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
            result._argsBuffer = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);
            result._argsBuffer.SetData(new[] {3, 0, 0, 0});
            result._trianglePositionCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk = new ComputeBuffer(512 * 8, 4);


            result._chunksToTriangulize = new ComputeBuffer(10000, 4 * 4, ComputeBufferType.Default);
            result._indexBufferCounter = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk.SetData(new int[] {result._triangleCountPerSubChunk.count});


            var requiredTriangleCapacity = Constants.subChunksPerCluster * 4 * 4 * 4 * 5;
            result._trianglePositionBuffer = new ComputeBuffer(requiredTriangleCapacity, 8 * 4, ComputeBufferType.Structured);

            result._triangulationIndices = new ComputeBuffer(requiredTriangleCapacity * 3, 4, ComputeBufferType.Structured);
            result._indexBuffer = new ComputeBuffer(requiredTriangleCapacity * 3, 4, ComputeBufferType.Append);
            
            result._clusterCounts = geometryFieldData.ClusterCounts;
            result._chunkCounts = geometryFieldData.ClusterCounts*Constants.chunkLengthPerCluster;
            result._propertyBlock = new MaterialPropertyBlock();
            
            return result;
        }

        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer,
            ComputeBuffer globalTerrainIndexMap,
            NativeSlice<CTriangulationInstruction> triangulationInstructions,
            NativeSlice<CSubChunkWithTrianglesIndex> cSubChunkWithTrianglesIndices,
            int materialIDFilter,
            CClusterParameters clusterParameters, int timeStamp,
            GPUVertexCountReadbackHandler gpuVertexCountReadbackHandler, Material defaultMaterial)
        {
            const int maxTrianglesPerSubChunk = 4 * 4 * 4 * 5;

            int indexCount;

            if (cSubChunkWithTrianglesIndices.Length == 0)
            {
                return;
            }
            else
            {
                // clusterParameters.lastVertexBufferChangeTimestamp = frameTimeStamp;

                var clusterPositionWS = clusterParameters.PositionWS;

                _indexBufferCounter.SetData(new uint[] {0, 1, 0, 0});

                if (triangulationInstructions.Length > 0)
                {
                    //todo copy directly from native array
                    //can probably make a nice extension method here
                    _chunksToTriangulize.SetData(triangulationInstructions.ToArray());

                    _trianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});

                    var resetTriangleCountKernel = _computeShader.FindKernel("ResetSubChunkTriangleCount");
                    _computeShader.SetBuffer(resetTriangleCountKernel, "_TerrainChunkBasePosition",
                        _chunksToTriangulize);
                    _computeShader.SetBuffer(resetTriangleCountKernel, "_TriangleCountPerSubChunk",
                        _triangleCountPerSubChunk);
                    _computeShader.Dispatch(resetTriangleCountKernel, triangulationInstructions.Length, 1, 1);


                    //Fine positions in the grid that contain triangles
                    var getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
                    _computeShader.SetInt("numPointsPerAxis", ChunkLength);
                    _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
                    _computeShader.SetInts("_TerrainMapSize", _chunkCounts.x, _chunkCounts.y, _chunkCounts.z);
                    _computeShader.SetBuffer(getPositionKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                    _computeShader.SetBuffer(getPositionKernel, "_ValidTrianglePositions", _trianglePositionBuffer);
                    _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
                    _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
                    _computeShader.SetBuffer(getPositionKernel, "_TriangleCountPerSubChunk", _triangleCountPerSubChunk);
                    _computeShader.SetBuffer(getPositionKernel, "_TriangleIndices", _triangulationIndices);
                    _trianglePositionBuffer.SetCounterValue(0);
                    _computeShader.Dispatch(getPositionKernel, triangulationInstructions.Length, 1, 1);
                    ComputeBuffer.CopyCount(_trianglePositionBuffer, _trianglePositionCountBuffer, 0);
                }
                
                
                if (clusterParameters.needsIndexBufferUpdate || true)
                {
                    _chunksToTriangulize.SetData(cSubChunkWithTrianglesIndices.ToArray());
                
                    var indexBufferKernel = _computeShader.FindKernel("BuildIndexBuffer");
                    _computeShader.SetBuffer(indexBufferKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                    _computeShader.SetBuffer(indexBufferKernel, "_TriangleCountPerSubChunkResult", _triangleCountPerSubChunk);
                    _computeShader.SetBuffer(indexBufferKernel, "_IndexBufferCounter", _indexBufferCounter);
                    _computeShader.SetBuffer(indexBufferKernel, "_ClusterMeshIndexBuffer", _indexBuffer);
                    _computeShader.SetBuffer(indexBufferKernel, "_AllVertexData", _triangulationIndices);
                    _computeShader.SetInts("_TerrainMapSize", _chunkCounts.x, _chunkCounts.y, _chunkCounts.z);
                    _computeShader.Dispatch(indexBufferKernel, cSubChunkWithTrianglesIndices.Length, 1, 1);
                }
                

                float4 extends = _clusterCounts.xyzz * Constants.clusterLength;

                _propertyBlock.SetBuffer("_TriangleIndeces", _triangulationIndices);
                _propertyBlock.SetInt("numPointsPerAxis", ChunkLength);
                _propertyBlock.SetInt("_MaterialIDFilter", materialIDFilter);
                _propertyBlock.SetVector("_DistanceFieldExtends", extends);
                _propertyBlock.SetBuffer("_GlobalTerrainIndexMap", globalTerrainIndexMap);
                _propertyBlock.SetBuffer("_GlobalTerrainBuffer", globalTerrainBuffer);
                _propertyBlock.SetBuffer("_ValidTrianglePositionResults", _trianglePositionBuffer);
                _propertyBlock.SetBuffer("_ArgsBuffer", _trianglePositionCountBuffer);


                Graphics.DrawProceduralIndirect(defaultMaterial, new Bounds(Vector3.zero, Vector3.one * 10000),
                    MeshTopology.Triangles, _indexBufferCounter, 0, null, _propertyBlock);
            }
        }


        public const int ChunkLength = 8;

 
        public void Dispose()
        {
            _argsBuffer.Dispose();
            _trianglePositionBuffer.Dispose();
            _chunksToTriangulize.Dispose();
            _trianglePositionCountBuffer.Dispose();
            _triangleCountPerSubChunk.Dispose();
            _indexBufferCounter.Dispose();
            _triangulationIndices.Dispose();
        }
    }
}