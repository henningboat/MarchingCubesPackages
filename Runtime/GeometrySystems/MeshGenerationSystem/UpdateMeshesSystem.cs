using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Rendering;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    internal struct UpdateMeshesSystem
    {
        private NativeArray<CTriangulationInstruction> _triangulationInstructions;
        private NativeArray<CSubChunkWithTrianglesIndex> _subChunksWithTrianglesData;
        private NativeArray<int> _vertexCountPerSubChunk;

        private GeometryFieldData _geometryFieldData;

        private ComputeBuffer _distanceFieldComputeBuffer;
        private ComputeBuffer _indexMapComputeBuffer;

        private ClusterMeshGPUBuffers[] _gpuDataPerCluster;
        private int _frameCount;

        private GPUVertexCountReadbackHandler _gpuReadbackHandler;


        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
            _triangulationInstructions =
                new NativeArray<CTriangulationInstruction>(geometryFieldData.TotalSubChunkCount, Allocator.Persistent);
            _subChunksWithTrianglesData =
                new NativeArray<CSubChunkWithTrianglesIndex>(geometryFieldData.TotalSubChunkCount,
                    Allocator.Persistent);
            _vertexCountPerSubChunk =
                new NativeArray<int>(geometryFieldData.TotalSubChunkCount, Allocator.Persistent);

            _distanceFieldComputeBuffer = new ComputeBuffer(_geometryFieldData.GeometryBuffer.Length, 4 * 4 * 2);
            _gpuReadbackHandler = new GPUVertexCountReadbackHandler(geometryFieldData);

            var indexMap = new int[_geometryFieldData.TotalChunkCount];
            for (var clusterIndex = 0; clusterIndex < _geometryFieldData.ClusterCount; clusterIndex++)
            {
                var cluster = _geometryFieldData.GetCluster(clusterIndex);
                for (var i = 0; i < Constants.chunksPerCluster; i++)
                {
                    var chunk = cluster.GetChunk(i);

                    //the whole index map is horrible legacy stuff I should get rid of
                    var totalFieldSizeGS = _geometryFieldData.ClusterCounts * Constants.chunkLengthPerCluster;
                    var indexInIndexMap =
                        TerrainChunkEntitySystem.Utils.PositionToIndex(
                            chunk.Parameters.PositionWS / Constants.chunkLengthPerCluster, totalFieldSizeGS);

                    indexMap[indexInIndexMap] = clusterIndex * Constants.chunksPerCluster + i;
                }
            }

            _indexMapComputeBuffer = new ComputeBuffer(indexMap.Length, 4);
            _indexMapComputeBuffer.SetData(indexMap);

            _gpuDataPerCluster = new ClusterMeshGPUBuffers[geometryFieldData.ClusterCount];
            for (var i = 0; i < geometryFieldData.ClusterCount; i++)
                _gpuDataPerCluster[i] = ClusterMeshGPUBuffers.CreateGPUData(geometryFieldData);
        }

        public void Update(JobHandle jobHandle)
        {
            _frameCount++;

            _gpuReadbackHandler.ApplyReadbacks(jobHandle, _vertexCountPerSubChunk);

            var calculateIndeicesJob = new JCalculateTriangulationIndicesJob
            {
                GeometryField = _geometryFieldData,
                TriangulationInstructions = _triangulationInstructions,
                VertexCountPerSubChunk = _vertexCountPerSubChunk,
                SubChunksWithTrianglesData = _subChunksWithTrianglesData
            };

            var handle = calculateIndeicesJob.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);
            handle.Complete();

            _distanceFieldComputeBuffer.SetData(_geometryFieldData.GeometryBuffer);

            for (var clusterIndex = 0; clusterIndex < _gpuDataPerCluster.Length; clusterIndex++)
            {
                var clusterParameters = _geometryFieldData.ClusterParameters[clusterIndex];

                var subChunkIndex = clusterIndex * Constants.subChunksPerCluster;
                var triangulationInstructions =
                    _triangulationInstructions.Slice(subChunkIndex, clusterParameters.triangulationInstructionCount);
                var subChunksWithTriangles =
                    _subChunksWithTrianglesData.Slice(subChunkIndex, clusterParameters.subChunksWithTrianglesCount);
                var gpuBuffers = _gpuDataPerCluster[clusterIndex];
             
                Debug.Log(triangulationInstructions.Length);
                
                gpuBuffers.UpdateWithSurfaceData(_distanceFieldComputeBuffer, _indexMapComputeBuffer,
                    triangulationInstructions, subChunksWithTriangles, 0,
                    clusterParameters, _frameCount, _gpuReadbackHandler);
            }
        }

        public void Dispose()
        {
            if (_gpuDataPerCluster != null)
                foreach (var meshGPUBuffers in _gpuDataPerCluster)
                    meshGPUBuffers.Dispose();

            if (_indexMapComputeBuffer != null) _indexMapComputeBuffer.Dispose();
            if (_distanceFieldComputeBuffer != null) _distanceFieldComputeBuffer.Dispose();

            _triangulationInstructions.Dispose();
            _subChunksWithTrianglesData.Dispose();
            _vertexCountPerSubChunk.Dispose();
            
            _gpuReadbackHandler.Dispose();
        }
    }
}