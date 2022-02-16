using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Rendering
{
    internal struct ClusterMeshGPUBuffers
    {
        //todo check the naming on all these buffers
        private ComputeShader _computeShader;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _trianglePositionCountBuffer;
        private ComputeBuffer _chunksToTriangulize;
        private ComputeBuffer _chunksWithTriangles;
        private ComputeBuffer _triangleCountPerSubChunk;
        private ComputeBuffer _indexBufferCounter;
        private ComputeBuffer _triangleBuffer;
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


            result._trianglePositionCountBuffer =
                new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk = new ComputeBuffer(512 * 8, 4);

            result._chunksToTriangulize =
                new ComputeBuffer(Constants.subChunksPerCluster, 4 * 4, ComputeBufferType.Default);
            result._chunksWithTriangles =
                new ComputeBuffer(Constants.subChunksPerCluster, 4 * 4, ComputeBufferType.Default);
            result._indexBufferCounter = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk.SetData(new[] {result._triangleCountPerSubChunk.count});


            var requiredTriangleCapacity = 64 * 64 * 64 * 5;

            result._triangulationIndices =
                new ComputeBuffer(requiredTriangleCapacity, 4, ComputeBufferType.Structured);
            result._triangleBuffer = new ComputeBuffer(requiredTriangleCapacity, 4, ComputeBufferType.Append);

            result._clusterCounts = geometryFieldData.ClusterCounts;
            result._chunkCounts = geometryFieldData.ClusterCounts * Constants.chunkLengthPerCluster;
            result._propertyBlock = new MaterialPropertyBlock();

            return result;
        }

        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer,
            ComputeBuffer globalTerrainIndexMap,
            NativeSlice<CTriangulationInstruction> triangulationInstructions,
            NativeSlice<CSubChunkWithTrianglesIndex> cSubChunkWithTrianglesIndices,
            int materialIDFilter,
            CClusterParameters clusterParameters, Material defaultMaterial)
        {
            if (cSubChunkWithTrianglesIndices.Length == 0) return;

            // clusterParameters.lastVertexBufferChangeTimestamp = frameTimeStamp;

            var clusterPositionWS = clusterParameters.PositionWS;

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
                _computeShader.SetInt("_TerrainMapSizeX", _chunkCounts.x);
                _computeShader.SetInt("_TerrainMapSizeY", _chunkCounts.y);
                _computeShader.SetInt("_TerrainMapSizeZ", _chunkCounts.z);
                _computeShader.SetInts("_ClusterPositionWS", clusterParameters.PositionWS.x,clusterParameters.PositionWS.y,clusterParameters.PositionWS.z);
                _computeShader.SetBuffer(getPositionKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
                _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
                _computeShader.SetBuffer(getPositionKernel, "_TriangleCountPerSubChunk", _triangleCountPerSubChunk);
                _computeShader.SetBuffer(getPositionKernel, "_TriangleIndices", _triangulationIndices);
                _computeShader.Dispatch(getPositionKernel, triangulationInstructions.Length, 1, 1);


                var dataReadback = new ClusterTriangle[_triangulationIndices.count];
                _triangulationIndices.GetData(dataReadback);
                

                _chunksWithTriangles.SetData(cSubChunkWithTrianglesIndices.ToArray());

                _indexBufferCounter.SetData(new uint[] {0, 1, 0, 0});

                var indexBufferKernel = _computeShader.FindKernel("BuildIndexBuffer");
                _computeShader.SetBuffer(indexBufferKernel, "_TerrainChunkBasePosition", _chunksWithTriangles);
                _computeShader.SetBuffer(indexBufferKernel, "_TriangleCountPerSubChunkResult",
                    _triangleCountPerSubChunk);
                _computeShader.SetBuffer(indexBufferKernel, "_IndexBufferCounter", _indexBufferCounter);
                _computeShader.SetBuffer(indexBufferKernel, "_ClusterMeshIndexBuffer", _triangleBuffer);
                _computeShader.SetBuffer(indexBufferKernel, "_AllVertexData", _triangulationIndices);

                _computeShader.SetInt("_TerrainMapSizeX", _chunkCounts.x);
                _computeShader.SetInt("_TerrainMapSizeY", _chunkCounts.y);
                _computeShader.SetInt("_TerrainMapSizeZ", _chunkCounts.z);

                _computeShader.SetInts("_TerrainMapSize", _chunkCounts.x, _chunkCounts.y, _chunkCounts.z);
                _computeShader.Dispatch(indexBufferKernel, cSubChunkWithTrianglesIndices.Length, 1, 1);
            }


            float4 extends = _clusterCounts.xyzz * Constants.clusterLength;

            _propertyBlock.SetBuffer("_TriangleIndeces", _triangleBuffer);
            _propertyBlock.SetInt("numPointsPerAxis", ChunkLength);
            _propertyBlock.SetInt("_MaterialIDFilter", materialIDFilter);
            _propertyBlock.SetVector("_DistanceFieldExtends", extends);
            _propertyBlock.SetBuffer("_GlobalTerrainIndexMap", globalTerrainIndexMap);
            _propertyBlock.SetBuffer("_GlobalTerrainBuffer", globalTerrainBuffer);
            _propertyBlock.SetInt("_TerrainMapSizeX", _chunkCounts.x);
            _propertyBlock.SetInt("_TerrainMapSizeY", _chunkCounts.y);
            _propertyBlock.SetInt("_TerrainMapSizeZ", _chunkCounts.z);
            
            _propertyBlock.SetInt("_PositionInClusterX", clusterParameters.PositionWS.x);
            _propertyBlock.SetInt("_PositionInClusterY", clusterParameters.PositionWS.y);
            _propertyBlock.SetInt("_PositionInClusterZ", clusterParameters.PositionWS.z);

            _propertyBlock.SetVector("_ClusterPositionWS", (Vector3) (float3) clusterParameters.PositionWS);


            Graphics.DrawProceduralIndirect(defaultMaterial, new Bounds(Vector3.zero, Vector3.one * 10000),
                MeshTopology.Triangles, _indexBufferCounter, 0, null, _propertyBlock);
        }


        public const int ChunkLength = 8;


        public void Dispose()
        {
            _argsBuffer.Dispose();
            _chunksToTriangulize.Dispose();
            _triangleCountPerSubChunk.Dispose();
            _indexBufferCounter.Dispose();
            _triangleBuffer.Dispose();
            _triangulationIndices.Dispose();
            _chunksWithTriangles.Dispose();
        }
    }
}