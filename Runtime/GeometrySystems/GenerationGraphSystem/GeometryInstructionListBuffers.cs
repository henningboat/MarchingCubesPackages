using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem
{
    public struct GeometryInstructionListBuffers : IDisposable
    {
        [NativeDisableParallelForRestriction] public NativeArray<float> ValueBuffer;
        [NativeDisableParallelForRestriction] public NativeArray<CMathInstruction> MathInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<GeometryInstruction> GeometryInstructions;

        public Hash128 ContentHash;
        public GeometryLayer TargetLayer;

        public GeometryInstructionListBuffers(GeometryInstructionList geometryGraphRuntimeData,
            GeometryLayer targetLayer)
        {
            ContentHash = geometryGraphRuntimeData.ContentHash;
            geometryGraphRuntimeData.AllocateNativeArrays(out ValueBuffer, out MathInstructions,
                out GeometryInstructions);
            IsValid = true;
            TargetLayer = targetLayer;
        }

        public bool IsValid { get; }

        public void Dispose()
        {
            Dispose(default);
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            inputDeps = ValueBuffer.Dispose(inputDeps);
            inputDeps = MathInstructions.Dispose(inputDeps);
            inputDeps = GeometryInstructions.Dispose(inputDeps);
            return inputDeps;
        }

        public void SetTopLevelBlendOperation(CombinerOperation combinerOperation)
        {
            for (var i = 0; i < GeometryInstructions.Length; i++)
            {
                var geometryInstruction = GeometryInstructions[i];
                if (geometryInstruction.CombinerDepth == 0)
                {
                    geometryInstruction.CombinerBlendOperation = combinerOperation;
                    GeometryInstructions[i] = geometryInstruction;
                }
            }
        }
    }
}