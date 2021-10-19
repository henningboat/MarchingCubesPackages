using System.Collections.Generic;
using Authoring;
using Rendering;
using Unity.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace NonECSImplementation
{
    internal struct UpdateMeshesSystem
    {
        private NativeArray<CTriangulationInstruction> _triangulationInstructions;
        private NativeArray<CSubChunkWithTrianglesIndex> _subChunksWithTrianglesData;
        private NativeArray<CSubChunkWithTrianglesIndex> _estimatedVertexCountPerSubChunk;
        private NativeArray<CVertexCountPerSubCluster> _vertexCountPerSubChunk;

        private NativeArray<int> _triangulationInstructionsPerCluster;
        private NativeArray<int> _subChunksWithTrianglesPerCluster;
        private GeometryFieldData _geometryFieldData;

        private ComputeBuffer _distanceFieldComputeBuffer;
        private ComputeBuffer _indexMapComputeBuffer;

        private ClusterMeshGPUBuffers[] _gpuDataPerCluster;


        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
            _triangulationInstructions =
                new NativeArray<CTriangulationInstruction>(geometryFieldData.TotalSubChunkCount, Allocator.Persistent);
            _subChunksWithTrianglesData =
                new NativeArray<CSubChunkWithTrianglesIndex>(geometryFieldData.TotalSubChunkCount,
                    Allocator.Persistent);
            _vertexCountPerSubChunk =
                new NativeArray<CVertexCountPerSubCluster>(geometryFieldData.TotalSubChunkCount, Allocator.Persistent);

            _gpuDataPerCluster = new ClusterMeshGPUBuffers[geometryFieldData.ClusterCount];
            for (int i = 0; i < geometryFieldData.ClusterCount; i++)
            {
                _gpuDataPerCluster[i] = ClusterMeshGPUBuffers.CreateGPUData(geometryFieldData);
            }
        }

        public void Update(JobHandle jobHandle)
        {
            JCalculateTriangulationIndicesJob job = new JCalculateTriangulationIndicesJob()
            {
                GeometryField = _geometryFieldData,
                TriangulationInstructions = _triangulationInstructions,
                VertexCountPerSubChunk = _vertexCountPerSubChunk,
                SubChunksWithTrianglesData = _subChunksWithTrianglesData,
            };

            var handle = job.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);
            handle.Complete();



            _distanceFieldComputeBuffer?.Dispose();
            _indexMapComputeBuffer?.Dispose();


            var terrainChunkDataBuffer = _geometryFieldData.GeometryBuffer;
            _distanceFieldComputeBuffer = new ComputeBuffer(terrainChunkDataBuffer.Length * 128, 4 * 4 * 2);
            _distanceFieldComputeBuffer.SetData(terrainChunkDataBuffer);

            //todo placeholder
            var indexMap = new int[_geometryFieldData.TotalChunkCount];
            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = i;
            }

            _indexMapComputeBuffer = new ComputeBuffer(indexMap.Length, 4);
            _indexMapComputeBuffer.SetData(indexMap);

            var clusterMeshRendererEntities = _gpuDataPerCluster;

            for (var clusterIndex = 0; clusterIndex < _gpuDataPerCluster.Length; clusterIndex++)
            {
                var clusterParameters = _geometryFieldData.ClusterParameters[clusterIndex];

                int subChunkIndex = clusterIndex * Constants.subChunksPerCluster;
                var triangulationInstructions =
                    _triangulationInstructions.Slice(subChunkIndex, clusterParameters.triangulationInstructionCount);
                var subChunksWithTriangles =
                    _subChunksWithTrianglesData.Slice(subChunkIndex, clusterParameters.subChunksWithTrianglesCount);
                var gpuBuffers = _gpuDataPerCluster[clusterIndex];

                //todo!
                int3 clusterPosition = 0;

                gpuBuffers.UpdateWithSurfaceData(_distanceFieldComputeBuffer, _indexMapComputeBuffer,
                    triangulationInstructions, subChunksWithTriangles, 0,
                    clusterPosition, clusterParameters);
            }
        }
    }
}