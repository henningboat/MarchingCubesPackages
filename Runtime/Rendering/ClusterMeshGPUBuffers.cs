using System;
using System.Collections.Generic;
using System.Linq;
using NonECSImplementation;
using TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;

namespace Rendering
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
        private NativeArray<int> _countPerSubChunkReadback;
        private Mesh _mesh;
        private int3 _clusterCounts;
        private int3 _chunkCounts;

        public static ClusterMeshGPUBuffers CreateGPUData(GeometryFieldData geometryFieldData)
        {
            ClusterMeshGPUBuffers result = default;
            result._computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
            result._argsBuffer = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);
            result._argsBuffer.SetData(new[] {3, 0, 0, 0});
            result._trianglePositionCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk = new ComputeBuffer(512 * 8, 4);


            result._chunksToTriangulize = new ComputeBuffer(10000, 4 * 4, ComputeBufferType.Default);
            result._indexBufferCounter = new ComputeBuffer(2, 4, default);

            result._triangleCountPerSubChunk.SetData(new[] {result._triangleCountPerSubChunk.count});

            result._countPerSubChunkReadback = new NativeArray<int>(512 * 8, Allocator.Persistent);
            
            
            var requiredTriangleCapacity = Constants.SubChunksInCluster * 4 * 4 * 4 * 5;
            result._trianglePositionBuffer = new ComputeBuffer(requiredTriangleCapacity, 8 * 4, ComputeBufferType.Append);

            result._mesh = MeshGeneratorBuilder.GenerateClusterMesh().mesh;

            result._clusterCounts = geometryFieldData.ClusterCounts;
            result._chunkCounts = geometryFieldData.ClusterCounts*NonECSImplementation.Constants.chunkLengthPerCluster;
            
            return result;
        }

        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer,
            ComputeBuffer globalTerrainIndexMap,
            NativeSlice<CTriangulationInstruction> triangulationInstructions,
            NativeSlice<CSubChunkWithTrianglesIndex> cSubChunkWithTrianglesIndices,
            int materialIDFilter, 
            int3 cluster,
            CClusterParameters clusterParameters)
        {
            const int maxTrianglesPerSubChunk = 4 * 4 * 4 * 5;

            int indexCount;

            if (cSubChunkWithTrianglesIndices.Length == 0)
            {
                indexCount = 0;
            }
            else
            {
               // clusterParameters.lastVertexBufferChangeTimestamp = frameTimeStamp;
                
                var clusterPositionWS = clusterParameters.PositionGS * 8;

                indexCount = math.min(_mesh.vertexCount, cSubChunkWithTrianglesIndices.Length * maxTrianglesPerSubChunk * 3);

                _indexBufferCounter.SetData(new[] {0, 0});
                
                if (triangulationInstructions.Length > 0)
                {
                    //todo copy directly from native array
                    //can probably make a nice extension method here
                    _chunksToTriangulize.SetData(triangulationInstructions.ToArray());

                    _trianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});

                    var resetTriangleCountKernel = _computeShader.FindKernel("ResetSubChunkTriangleCount");
                    _computeShader.SetBuffer(resetTriangleCountKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                    _computeShader.SetBuffer(resetTriangleCountKernel, "_TriangleCountPerSubChunk", _triangleCountPerSubChunk);
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
                    _trianglePositionBuffer.SetCounterValue(0);
                    _computeShader.Dispatch(getPositionKernel, triangulationInstructions.Length, 1, 1);
                    ComputeBuffer.CopyCount(_trianglePositionBuffer, _trianglePositionCountBuffer, 0);

                    var meshVertexBuffer = _mesh.GetVertexBuffer(0);

                    // var clearVertexData = _computeShader.FindKernel("ClearVertexData");
                    // _computeShader.SetBuffer(clearVertexData, "triangles", meshVertexBuffer);
                    // _computeShader.Dispatch(clearVertexData, mesh.vertexCount / 512, 1, 1);

                    var calculateTriangulationThreadGroupSizeKernel = _computeShader.FindKernel("CalculateTriangulationThreadGroupSizeKernel");
                    _computeShader.SetBuffer(calculateTriangulationThreadGroupSizeKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
                    _computeShader.Dispatch(calculateTriangulationThreadGroupSizeKernel, 1, 1, 1);

                    var triangulationKernel = _computeShader.FindKernel("Triangulation");
                    _computeShader.SetInt("numPointsPerAxis", ChunkLength);
                    _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
                    _computeShader.SetInts("_TerrainMapSize", _chunkCounts.x, _chunkCounts.y, _chunkCounts.z);

                    _computeShader.SetBuffer(triangulationKernel, "triangles", meshVertexBuffer);
                    _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
                    _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
                    _computeShader.SetBuffer(triangulationKernel, "_ValidTrianglePositionResults", _trianglePositionBuffer);
                    _computeShader.SetBuffer(triangulationKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
                    _computeShader.SetInts("_ClusterPositionWS", clusterPositionWS.x, clusterPositionWS.y, clusterPositionWS.z);
                    _computeShader.DispatchIndirect(triangulationKernel, _trianglePositionCountBuffer, 4);
                    meshVertexBuffer.Dispose();
                }
            }

//todo
            if (clusterParameters.needsIndexBufferUpdate || true)
            {
                var meshIndexBuffer = _mesh.GetIndexBuffer();
                _chunksToTriangulize.SetData(cSubChunkWithTrianglesIndices.ToArray());

                var indexBufferKernel = _computeShader.FindKernel("BuildIndexBuffer");
                _computeShader.SetBuffer(indexBufferKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                _computeShader.SetBuffer(indexBufferKernel, "_TriangleCountPerSubChunkResult", _triangleCountPerSubChunk);
                _computeShader.SetBuffer(indexBufferKernel, "_IndexBufferCounter", _indexBufferCounter);
                _computeShader.SetBuffer(indexBufferKernel, "_ClusterMeshIndexBuffer", meshIndexBuffer);
                _computeShader.SetInt("_IndexBufferSize", indexCount);
                _computeShader.SetInts("_TerrainMapSize", _chunkCounts.x, _chunkCounts.y, _chunkCounts.z);
                _computeShader.Dispatch(indexBufferKernel, cSubChunkWithTrianglesIndices.Length, 1, 1);

                meshIndexBuffer.Dispose();
            }

            //todo reimplement
           // AsyncReadbackUtility.AddCallbackIfNeeded(cluster.ClusterIndex, _triangleCountPerSubChunk, frameTimeStamp, clusterParameters.lastVertexBufferChangeTimestamp);


            _mesh.SetSubMeshes(new[] {new SubMeshDescriptor(0, clusterParameters.vertexCount)}, MeshGeneratorBuilder.MeshUpdateFlagsNone);

            Graphics.DrawMesh(_mesh, Matrix4x4.identity, DynamicCubeMarchingSettingsHolder.Instance.Materials.FirstOrDefault(), 0);
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
            _countPerSubChunkReadback.Dispose();
        }
    }
}