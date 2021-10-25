using System;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Utils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
{
    public class DistanceFieldPrepassSystem
    {
        private GeometryFieldData _geometryFieldData;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }
        
        public JobHandle Update(GeometryGraphData graph, JobHandle jobHandle)
        {
            var hashingJob = new JHashJob(graph);
            jobHandle = hashingJob.Schedule(graph.GeometryInstructions.Length, 32, jobHandle);
            
            var job = new JExecuteDistanceFieldPrepass(_geometryFieldData, graph);
            jobHandle = job.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);
            // jobHandle.Complete();
            // int count = 0;
            //
            // var cluster = _geometryFieldData.GetCluster(0);
            // for (int i = 0; i < 512; i++)
            // {
            //     if (cluster.Parameters.WriteMask[i])
            //     {
            //         count++;
            //         int3 positionWS = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 8) * 8;
            //         Debug.DrawRay((float3) positionWS, Vector3.right*8);
            //         Debug.DrawRay((float3) positionWS, Vector3.up*8);
            //         Debug.DrawRay((float3) positionWS, Vector3.forward*8);
            //     }
            //  }
            // Debug.Log(count);
            return jobHandle;
        }
    }
}