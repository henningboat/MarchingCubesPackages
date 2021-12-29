using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal class GPUVertexCountReadbackHandler
    {
        private NativeArray<int> _vertexCountPerSubChunkReadback;
        private NativeArray<int> _readbackTimeStampPerCluster;
        private GeometryFieldData _geometryFieldData;

        public GPUVertexCountReadbackHandler(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
            _vertexCountPerSubChunkReadback = new NativeArray<int>(
                geometryFieldData.ClusterCount * Constants.subChunksPerCluster,
                Allocator.Persistent);
            _readbackTimeStampPerCluster = new NativeArray<int>(geometryFieldData.ClusterCount, Allocator.Persistent);
        }

        public void Dispose()
        {
            _vertexCountPerSubChunkReadback.Dispose();
            _readbackTimeStampPerCluster.Dispose();
        }

        public JobHandle ApplyReadbacks(JobHandle jobHandle, NativeArray<int> vertexCountPerSubChunk)
        {
            JApplyGPUVertexCountReadBacks job = new JApplyGPUVertexCountReadBacks()
            {
                GeometryFieldData = _geometryFieldData,
                ReadbackTimeStampPerCluster = _readbackTimeStampPerCluster,
                VertexCountPerSubChunk = vertexCountPerSubChunk,
                VertexCountPerSubChunkReadback = _vertexCountPerSubChunkReadback
            };

            jobHandle = job.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);

            return jobHandle;
        }

        public void RequestReadback(CClusterParameters clusterParameters, ComputeBuffer vertexCountComputeBuffer, int timeStamp)
        {
            int clusterIndex = clusterParameters.ClusterIndex;
            int timeStampOfRequest = timeStamp;
            
            AsyncGPUReadback.Request(vertexCountComputeBuffer, request =>
            {
                if (request.hasError)
                    Debug.LogWarning("Could not receive vertex count of cluster " + clusterParameters.ClusterIndex);

                //since this code is executed async, we might be in a new scene already
                if (_vertexCountPerSubChunkReadback.IsCreated == false)
                {
                    return;
                }
                
                var targetSlice = _vertexCountPerSubChunkReadback.Slice(clusterIndex * Constants.subChunksPerCluster,
                    Constants.subChunksPerCluster);
                var data = request.GetData<int>();

                targetSlice.CopyFrom(data);
                
                _readbackTimeStampPerCluster[clusterIndex] = timeStampOfRequest;

                targetSlice.CopyFrom(data);
                
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Debug.Log("queue update");
                    EditorApplication.QueuePlayerLoopUpdate();
                    UnityEditor.SceneView.RepaintAll();
                }
#endif
            });
        }
    }
}