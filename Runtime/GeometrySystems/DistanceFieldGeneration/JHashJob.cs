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
            var valueBuffer = _graph.ValueBuffer;
            var instruction = _graph.GeometryInstructions[index];

            var hash = new Hash128();
            //todo
//            hash.Append(ref instruction);

            // for (var i = 0; i < 16; i++)
            // {
            //     hash.Append(ref instruction.ResolvedPropertyValues);
            // }
            //
            // for (var i = 0; i < 16; i++) hash.Append(valueBuffer[i + instruction.TransformationValue.Index]);

            _graph.HashPerInstruction[index] = hash;
        }
    }
}