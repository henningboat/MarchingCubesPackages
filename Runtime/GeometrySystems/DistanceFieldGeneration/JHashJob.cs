using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    public struct JHashJob : IJobParallelFor
    {
        private GeometryGraphData _graph;

        public JHashJob(GeometryGraphData graph)
        {
            _graph = graph;
        }

        public void Execute(int index)
        {
            var instruction = _graph.GeometryInstructions[index];

            var hash = new Hash128();
            hash.Append(ref instruction);
            _graph.HashPerInstruction[index] = hash;
        }
    }
}