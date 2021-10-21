using GeometrySystems.GeometryFieldSetup;
using NonECSImplementation;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GeometrySystems.DistanceFieldGeneration
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
            NativeArray<bool4> culling = new NativeArray<bool4>(128, Allocator.TempJob);
            var job = new JExecuteDistanceFieldPrepass(_geometryFieldData, graph,culling);
            jobHandle = job.Schedule(_geometryFieldData.ClusterCount, 1, jobHandle);
            jobHandle.Complete();
            int count = 0;
            for (int i = 0; i < 512; i++)
            {
                if (culling[i / 4][i % 4])
                {
                    count++;
                    int3 positionWS = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 8) * 8;

                    Debug.DrawRay((float3) positionWS, Vector3.right);
                }
            }
            Debug.Log(count);
            culling.Dispose();
            return jobHandle;
        }
    }
}