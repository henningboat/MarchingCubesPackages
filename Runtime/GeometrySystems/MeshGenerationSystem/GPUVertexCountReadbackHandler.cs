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

        public GPUVertexCountReadbackHandler(GeometryFieldData geometryFieldData)
        {
            _vertexCountPerSubChunkReadback = new NativeArray<int>(
                geometryFieldData.ClusterCount * Constants.subChunksPerCluster,
                Allocator.Persistent);
        }

        public void Dispose()
        {
            _vertexCountPerSubChunkReadback.Dispose();
        }

        public JobHandle ApplyReadbacks(JobHandle jobHandle, NativeArray<int> vertexCountPerSubChunk)
        {
            //todo turn into job
            jobHandle.Complete();
            vertexCountPerSubChunk.CopyFrom(_vertexCountPerSubChunkReadback);
            return jobHandle;
        }
        
        public void RequestReadback(CClusterParameters clusterParameters, ComputeBuffer vertexCountComputeBuffer)
        {
            return;
            int clusterIndex = clusterParameters.ClusterIndex;
            
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