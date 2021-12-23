using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration;
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
            var allGraphs = Object.FindObjectsOfType<GeometryInstanceBase>();
            foreach (var graphInstance in allGraphs)
            {
                graphInstance.InitializeGraphDataIfNeeded();

                graphInstance.UpdateOverwrites();
                
                var updateGraphMathJob = new JUpdateGraphMath(graphInstance.GraphBuffers);
                var graphInstanceJobHandle = updateGraphMathJob.Schedule();

                var writeValuesJob = new JWriteValueBufferToInstruction(graphInstance.GraphBuffers);
                graphInstanceJobHandle = writeValuesJob.Schedule(graphInstance.GraphBuffers.GeometryInstructions.Length,
                    JWriteValueBufferToInstruction.InnerGroupSize, graphInstanceJobHandle);

                var updateHashesJob = new JHashJob(graphInstance.GraphBuffers);
                graphInstanceJobHandle = updateHashesJob.Schedule(graphInstance.GraphBuffers.GeometryInstructions.Length, 32,
                    graphInstanceJobHandle);
                
                jobHandle = JobHandle.CombineDependencies(jobHandle, graphInstanceJobHandle);
            }

            return jobHandle;
        }
    }

    [BurstCompile]
    public struct JUpdateGraphMath : IJob
    {
        private GeometryGraphBuffers _graph;

        public JUpdateGraphMath(GeometryGraphBuffers graph)
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
    
    [BurstCompile]
    public struct JWriteValueBufferToInstruction : IJobParallelFor
    {
        public const int InnerGroupSize = 64;
        private GeometryGraphBuffers _graph;

        public JWriteValueBufferToInstruction(GeometryGraphBuffers graph)
        {
            _graph = graph;
        }

        public void Execute(int index)
        {
            for (var instructionIndex = 0; instructionIndex < _graph.GeometryInstructions.Length; instructionIndex++)
            {
                var instruction = _graph.GeometryInstructions[instructionIndex];
                
                for (int i = 0; i < 32; i++)
                {
                    instruction.ResolvedPropertyValues[i] = _graph.ValueBuffer[instruction.PropertyIndexes[i]];
                }

                _graph.GeometryInstructions[instructionIndex] = instruction;
            }
        }
    }
    
}