using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
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
            //todo turn into job
            jobHandle.Complete();

            for (int clusterIndex = 0; clusterIndex < _geometryFieldData.ClusterCount; clusterIndex++)
            {
                int timeStampOfReadback = _readbackTimeStampPerCluster[clusterIndex];
                var cluster = _geometryFieldData.GetCluster(clusterIndex);

                for (int chunkIndex = 0; chunkIndex < Constants.chunksPerCluster; chunkIndex++)
                {
                    var chunk = cluster.GetChunk(chunkIndex);
                    if (chunk.Parameters.InstructionChangeTimeStamp <= timeStampOfReadback)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            var subChunkIndex = clusterIndex*Constants.subChunksPerCluster+chunkIndex*Constants.subChunksPerChunk+i;
                            vertexCountPerSubChunk[subChunkIndex] = _vertexCountPerSubChunkReadback[subChunkIndex];
                        }
                    }
                }
            }

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
                
                
                //todo find out where the negative numbers come from
                for (int i = 0; i < targetSlice.Length; i++)
                {
                    targetSlice[i] = math.max(0, targetSlice[i]);
                }

                _readbackTimeStampPerCluster[clusterIndex] = timeStampOfRequest;
                
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