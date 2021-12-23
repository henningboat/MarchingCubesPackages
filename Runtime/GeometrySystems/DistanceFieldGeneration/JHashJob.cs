using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    public struct JHashJob : IJobParallelFor
    {
        private GeometryGraphBuffers _graph;

        public JHashJob(GeometryGraphBuffers graph)
        {
            _graph = graph;
        }

        public void Execute(int index)
        {
            var instruction = _graph.GeometryInstructions[index];
            instruction.UpdateHash();
            _graph.GeometryInstructions[index] = instruction;
        }
    }
}