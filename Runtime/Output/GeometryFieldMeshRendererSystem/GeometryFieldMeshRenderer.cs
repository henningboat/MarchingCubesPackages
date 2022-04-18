using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.Output.GeometryFieldMeshRendererSystem;
using henningboat.CubeMarching.Runtime.Rendering;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public class GeometryFieldMeshRenderer : MonoBehaviour, IGeometryFieldReceiver
    {
        [SerializeField] private GeometryLayerAsset _geometryLayerAsset;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private int _layer;
        
        private NativeList<int> _chunksModifiedThisFrame;

        private ComputeBuffer _distanceFieldComputeBuffer;

        private GeometryFieldData _geometryFieldData;

        private GeometryLayer _geometryLayer;

        private ClusterMeshGPUBuffers[] _gpuDataPerCluster;

        private ComputeBuffer _indexMapComputeBuffer;
        private NativeArray<CSubChunkWithTrianglesIndex> _subChunksWithTrianglesData;
        private NativeArray<CTriangulationInstruction> _triangulationInstructions;
        private NativeArray<int> _vertexCountPerSubChunk;

        public Material DefaultMaterial
        {
            get => _defaultMaterial;
            set => _defaultMaterial = value;
        }

        private void OnDisable()
        {
            Dispose();
        }

        public void OnJobsFinished(GeometryFieldData geometryFieldData)
        {
            // if (_chunksModifiedThisFrame.Length > 0)
            // {
            //     var computeBufferNativeArray =
            //         _distanceFieldComputeBuffer.BeginWrite<PackedDistanceFieldData>(0,
            //             _geometryFieldData.GeometryBuffer.Length);
            //     foreach (var chunkToUpload in _chunksModifiedThisFrame)
            //     {
            //         var packedChunkSize = Constants.chunkVolume / Constants.PackedCapacity;
            //         var chunkIndex = chunkToUpload * packedChunkSize;
            //         computeBufferNativeArray.CopyFrom(_geometryFieldData.GeometryBuffer, chunkIndex, chunkIndex,
            //             packedChunkSize);
            //     }
            //
            //     _distanceFieldComputeBuffer.EndWrite<PackedDistanceFieldData>(_geometryFieldData.GeometryBuffer.Length);
            // }

            //todo placeholder, it's probably better to only copy modified parts
            if (_chunksModifiedThisFrame.Length > 0)
            {
                var computeBufferNativeArray =
                    _distanceFieldComputeBuffer.BeginWrite<PackedDistanceFieldData>(0,
                        _geometryFieldData.GeometryBuffer.Length);
                computeBufferNativeArray.CopyFrom(_geometryFieldData.GeometryBuffer);
                
                _distanceFieldComputeBuffer.EndWrite<PackedDistanceFieldData>(_geometryFieldData.GeometryBuffer.Length);
            }
            
            for (var clusterIndex = 0; clusterIndex < _gpuDataPerCluster.Length; clusterIndex++)
            {
                var clusterParameters = _geometryFieldData.ClusterParameters[clusterIndex];

                var subChunkIndex = clusterIndex * Constants.subChunksPerCluster;
                var triangulationInstructions =
                    _triangulationInstructions.Slice(subChunkIndex, clusterParameters.triangulationInstructionCount);
                var subChunksWithTriangles =
                    _subChunksWithTrianglesData.Slice(subChunkIndex, clusterParameters.subChunksWithTrianglesCount);
                var gpuBuffers = _gpuDataPerCluster[clusterIndex];

                gpuBuffers.UpdateWithSurfaceData(_distanceFieldComputeBuffer, _indexMapComputeBuffer,
                    triangulationInstructions, subChunksWithTriangles, 0,
                    clusterParameters, _defaultMaterial,_layer);
            }
        }

        public GeometryLayer RequestedLayer()
        {
            return _geometryLayerAsset != null ? _geometryLayerAsset.GeometryLayer : GeometryLayer.OutputLayer;
        }

        public JobHandle ScheduleJobs(JobHandle jobHandle, GeometryFieldData requestedField,
            NativeList<int> chunksModifiedThisFrame)
        {
            _chunksModifiedThisFrame = chunksModifiedThisFrame;
            var calculateIndeicesJob = new JCalculateTriangulationIndicesJob
            {
                GeometryField = _geometryFieldData,
                TriangulationInstructions = _triangulationInstructions,
                VertexCountPerSubChunk = _vertexCountPerSubChunk,
                SubChunksWithTrianglesData = _subChunksWithTrianglesData
            };

            jobHandle = calculateIndeicesJob.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);

            return jobHandle;
        }

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

            _distanceFieldComputeBuffer = new ComputeBuffer(_geometryFieldData.GeometryBuffer.Length, 4 * 4 * 2,
                ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

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

        public void Dispose()
        {
            if (_gpuDataPerCluster != null)
                foreach (var meshGPUBuffers in _gpuDataPerCluster)
                    meshGPUBuffers.Dispose();

            if (_indexMapComputeBuffer != null) _indexMapComputeBuffer.Dispose();
            if (_distanceFieldComputeBuffer != null) _distanceFieldComputeBuffer.Dispose();

            if (_triangulationInstructions.IsCreated) _triangulationInstructions.Dispose();
            if (_subChunksWithTrianglesData.IsCreated) _subChunksWithTrianglesData.Dispose();
            if (_vertexCountPerSubChunk.IsCreated) _vertexCountPerSubChunk.Dispose();
        }
    }

    [BurstCompile]
    internal struct JGetChunksWithModifications : IJobParallelFor
    {
        [ReadOnly] public GeometryFieldData GeometryField;
        [WriteOnly] public NativeList<int>.ParallelWriter ChunksWithModifiedIndices;

        public void Execute(int chunkIndex)
        {
            var chunk = GeometryField.GetChunk(chunkIndex);
            if (GeometryChunkParameters.InstructionsChangedSinceLastFrame)
                ChunksWithModifiedIndices.AddNoResize(chunkIndex);
        }
    }
}