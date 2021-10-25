using System;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem
{
    public struct GeometryGraphData : IDisposable
    {
        [NativeDisableParallelForRestriction] public NativeArray<float> ValueBuffer;
        [NativeDisableParallelForRestriction] public NativeArray<MathInstruction> MathInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<GeometryInstruction> GeometryInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<Hash128> HashPerInstruction;

        public Hash128 ContentHash;

        public GeometryGraphData(GeometryGraphRuntimeData geometryGraphRuntimeData)
        {
            ContentHash = geometryGraphRuntimeData.ContentHash;
            geometryGraphRuntimeData.AllocateNativeArrays(out ValueBuffer, out MathInstructions,
                out GeometryInstructions);
            HashPerInstruction = new NativeArray<Hash128>(GeometryInstructions.Length, Allocator.Persistent);
        }

        public void Dispose()
        {
            Dispose(default);
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            inputDeps = ValueBuffer.Dispose(inputDeps);
            inputDeps = MathInstructions.Dispose(inputDeps);
            inputDeps = GeometryInstructions.Dispose(inputDeps);
            inputDeps = HashPerInstruction.Dispose(inputDeps);
            return inputDeps;
        }
    }
}