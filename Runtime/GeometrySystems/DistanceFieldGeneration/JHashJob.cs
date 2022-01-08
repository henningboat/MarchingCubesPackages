using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using Unity.Burst;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    public struct JHashJob : IJobParallelFor
    {
        private GeometryInstructionListBuffers _instructionList;

        public JHashJob(GeometryInstructionListBuffers instructionList)
        {
            _instructionList = instructionList;
        }

        public void Execute(int index)
        {
            var instruction = _instructionList.GeometryInstructions[index];
            instruction.UpdateHash();
            _instructionList.GeometryInstructions[index] = instruction;
        }
    }
}