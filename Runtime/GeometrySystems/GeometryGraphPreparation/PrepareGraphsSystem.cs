using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation
{
    public class PrepareGraphsSystem
    {
        public void Initialize()
        {
        }

        public JobHandle Update(JobHandle jobHandle)
        {
            var allGraphs = Object.FindObjectsOfType<GeometryGraphInstance>();
            foreach (var graphInstance in allGraphs)
            {
                graphInstance.UpdateOverwrites();

                var job = new JUpdateGraphMath(graphInstance.GetGraphData());
                var graphInstanceJobHandle = job.Schedule();
                jobHandle = JobHandle.CombineDependencies(jobHandle, graphInstanceJobHandle);
            }

            return jobHandle;
        }
    }

    [BurstCompile]
    public struct JUpdateGraphMath : IJob
    {
        private GeometryGraphData _graph;

        public JUpdateGraphMath(GeometryGraphData graph)
        {
            _graph = graph;
        }

        public void Execute()
        {
            for (var i = 0; i < _graph.MathInstructions.Length; i++)
            {
                var instruction = _graph.MathInstructions[i];
                instruction.Execute(_graph.ValueBuffer);
            }
        }
    }
}