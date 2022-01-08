using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem
{
    public struct GeometryInstructionListBuffers : IDisposable
    {
        [NativeDisableParallelForRestriction] public NativeArray<float> ValueBuffer;
        [NativeDisableParallelForRestriction] public NativeArray<MathInstruction> MathInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<GeometryInstruction> GeometryInstructions;

        public Hash128 ContentHash;
        public SerializableGUID TargetLayerID;

        public GeometryInstructionListBuffers(GeometryInstructionList geometryGraphRuntimeData,
            SerializableGUID targetLayerID)
        {
            ContentHash = geometryGraphRuntimeData.ContentHash;
            geometryGraphRuntimeData.AllocateNativeArrays(out ValueBuffer, out MathInstructions,
                out GeometryInstructions);
            IsValid = true;
            TargetLayerID = targetLayerID;
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
    }
}