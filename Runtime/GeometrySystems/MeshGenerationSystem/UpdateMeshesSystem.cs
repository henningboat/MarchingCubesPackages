using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.Rendering;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal struct UpdateMeshesSystem
    {
        private NativeArray<CTriangulationInstruction> _triangulationInstructions;
        private NativeArray<CSubChunkWithTrianglesIndex> _subChunksWithTrianglesData;
        private NativeArray<int> _vertexCountPerSubChunk;
        private NativeArray<PackedDistanceFieldData> _gpuTransferBuffer;

        private GeometryFieldData _geometryFieldData;

        private ComputeBuffer _distanceFieldComputeBuffer;
        private ComputeBuffer _gpuTransferComputeBuffer;
        private ComputeBuffer _indexMapComputeBuffer;
        private ComputeBuffer _gpuTransferIndexMap;

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
            _gpuTransferComputeBuffer =
                new ComputeBuffer(_geometryFieldData.GeometryBuffer.Length, 4 * 4 * 2);
            _gpuTransferIndexMap = new ComputeBuffer(geometryFieldData.TotalChunkCount, sizeof(int));
            _gpuReadbackHandler = new GPUVertexCountReadbackHandler(geometryFieldData);

            _gpuTransferBuffer =
                new NativeArray<PackedDistanceFieldData>(_geometryFieldData.GeometryBuffer.Length,
                    Allocator.Persistent);

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
                        Runtime.DistanceFieldGeneration.Utils.PositionToIndex(
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

            var chhunksToUploadToGPU =
                new NativeList<int>(_geometryFieldData.TotalChunkCount, Allocator.TempJob);

            var extractModifiedChunksJob = new JExtractModifiedChunks
            {
                GeometryField = _geometryFieldData,
                ChunksWithModifiedIndices = chhunksToUploadToGPU.AsParallelWriter()
            };

            jobHandle = extractModifiedChunksJob.Schedule(_geometryFieldData.TotalChunkCount, 64, jobHandle);

            var copyChunksToGPUTransferBufferJob = new JCopyChunksToGPUTransferBuffer
            {
                _gpuTransferBuffer = _gpuTransferBuffer,
                GeometryField = _geometryFieldData,
                _chunksWithModifiedIndices = chhunksToUploadToGPU
            };

            jobHandle = copyChunksToGPUTransferBufferJob.Schedule(_geometryFieldData.TotalChunkCount, 16, jobHandle);

            jobHandle = _gpuReadbackHandler.ApplyReadbacks(jobHandle, _vertexCountPerSubChunk);

            var calculateIndeicesJob = new JCalculateTriangulationIndicesJob
            {
                GeometryField = _geometryFieldData,
                TriangulationInstructions = _triangulationInstructions,
                VertexCountPerSubChunk = _vertexCountPerSubChunk,
                SubChunksWithTrianglesData = _subChunksWithTrianglesData
            };

            var handle = calculateIndeicesJob.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);
            handle.Complete();

            if (chhunksToUploadToGPU.Length > 0)
            {
                var computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
                int writeFromTransferToGlobalBufferKernel = computeShader.FindKernel("WriteFromTransferToGlobalBuffer");

                _gpuTransferComputeBuffer.SetData(_gpuTransferBuffer, 0, 0, 128 * chhunksToUploadToGPU.Length);
                _gpuTransferIndexMap.SetData(chhunksToUploadToGPU.AsArray());

                computeShader.SetBuffer(writeFromTransferToGlobalBufferKernel, "_RWGlobalTerrainBuffer",
                    _distanceFieldComputeBuffer);
                computeShader.SetBuffer(writeFromTransferToGlobalBufferKernel, "_ChunksWithChangedInstructions",
                    _gpuTransferIndexMap);
                computeShader.SetBuffer(writeFromTransferToGlobalBufferKernel, "_TransferBuffer",
                    _gpuTransferComputeBuffer);

                computeShader.Dispatch(writeFromTransferToGlobalBufferKernel, chhunksToUploadToGPU.Length, 1, 1);
            }

            chhunksToUploadToGPU.Dispose();

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
            if (_gpuTransferComputeBuffer != null) _gpuTransferComputeBuffer.Dispose();
            if (_gpuTransferIndexMap != null) _gpuTransferIndexMap.Dispose();
            
            _gpuTransferBuffer.Dispose();

            _triangulationInstructions.Dispose();
            _subChunksWithTrianglesData.Dispose();
            _vertexCountPerSubChunk.Dispose();

            _gpuReadbackHandler.Dispose();
        }
    }

    [BurstCompile]
    internal struct JExtractModifiedChunks : IJobParallelFor
    {
        public GeometryFieldData GeometryField;
        [WriteOnly] public NativeList<int>.ParallelWriter ChunksWithModifiedIndices;

        public void Execute(int chunkIndex)
        {
            var chunk = GeometryField.GetChunk(chunkIndex);
            if (chunk.Parameters.InstructionsChangedSinceLastFrame && chunk.Parameters.HasData)
                ChunksWithModifiedIndices.AddNoResize(chunkIndex);
        }
    }

    [BurstCompile]
    internal struct JCopyChunksToGPUTransferBuffer : IJobParallelFor
    {
        [ReadOnly] public NativeList<int> _chunksWithModifiedIndices;
        [NativeDisableParallelForRestriction] public NativeArray<PackedDistanceFieldData> _gpuTransferBuffer;
        public GeometryFieldData GeometryField;

        public void Execute(int index)
        {
            if (index >= _chunksWithModifiedIndices.Length) return;

            var packedChunkVolume = Constants.chunkVolume / Constants.PackedCapacity;

            var sourceSlice = GeometryField.GeometryBuffer.Slice(packedChunkVolume * _chunksWithModifiedIndices[index],
                packedChunkVolume);
            var targetSlice = _gpuTransferBuffer.Slice(packedChunkVolume * index, packedChunkVolume);

            //todo benchmark if a memcpy is faster
            for (var i = 0; i < targetSlice.Length; i++) targetSlice[i] = sourceSlice[i];
        }
    }
}