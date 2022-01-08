using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using Unity.Burst;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation
{
    public class PrepareGraphsSystem
    {
        public void Initialize()
        {
        }

        public JobHandle Update(JobHandle jobHandle, List<GeometryInstructionListBuffers> allGraphs)
        {
            foreach (var graphBuffers in allGraphs)
            {
                var updateGraphMathJob = new JUpdateGraphMath(graphBuffers);
                var graphInstanceJobHandle = updateGraphMathJob.Schedule();

                var writeValuesJob = new JWriteValueBufferToInstruction(graphBuffers);
                graphInstanceJobHandle = writeValuesJob.Schedule(graphBuffers.GeometryInstructions.Length,
                    JWriteValueBufferToInstruction.InnerGroupSize, graphInstanceJobHandle);

                var updateHashesJob = new JHashJob(graphBuffers);
                graphInstanceJobHandle = updateHashesJob.Schedule(graphBuffers.GeometryInstructions.Length, 32,
                    graphInstanceJobHandle);

                jobHandle = JobHandle.CombineDependencies(jobHandle, graphInstanceJobHandle);
            }

            return jobHandle;
        }
    }

    [BurstCompile]
    public struct JUpdateGraphMath : IJob
    {
        private GeometryInstructionListBuffers _instructionList;

        public JUpdateGraphMath(GeometryInstructionListBuffers instructionList)
        {
            _instructionList = instructionList;
        }

        public void Execute()
        {
            for (var i = 0; i < _instructionList.MathInstructions.Length; i++)
            {
                var instruction = _instructionList.MathInstructions[i];
                instruction.Execute(_instructionList.ValueBuffer);
            }
        }
    }

    [BurstCompile]
    public struct JWriteValueBufferToInstruction : IJobParallelFor
    {
        public const int InnerGroupSize = 64;
        private GeometryInstructionListBuffers _instructionList;

        public JWriteValueBufferToInstruction(GeometryInstructionListBuffers instructionList)
        {
            _instructionList = instructionList;
        }

        public void Execute(int index)
        {
            for (var instructionIndex = 0;
                 instructionIndex < _instructionList.GeometryInstructions.Length;
                 instructionIndex++)
            {
                var instruction = _instructionList.GeometryInstructions[instructionIndex];

                for (var i = 0; i < 32; i++)
                    instruction.ResolvedPropertyValues[i] =
                        _instructionList.ValueBuffer[instruction.PropertyIndexes[i]];

                _instructionList.GeometryInstructions[instructionIndex] = instruction;
            }
        }
    }
}