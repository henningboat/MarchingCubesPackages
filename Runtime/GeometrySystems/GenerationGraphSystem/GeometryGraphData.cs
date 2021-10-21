using System;
using GeometrySystems.GeometryMath;
using TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace NonECSImplementation
{
    public struct GeometryGraphData : IDisposable
    {
        [NativeDisableParallelForRestriction] public NativeArray<float> ValueBuffer;
        [NativeDisableParallelForRestriction] public NativeArray<MathInstruction> MathInstructions;
        [NativeDisableParallelForRestriction] public NativeArray<GeometryInstruction> GeometryInstructions;

        public readonly Hash128 ContentHash;

        public GeometryGraphData(GeometryGraphRuntimeData geometryGraphRuntimeData)
        {
            ContentHash = geometryGraphRuntimeData.ContentHash;
            geometryGraphRuntimeData.AllocateNativeArrays(out ValueBuffer, out MathInstructions,
                out GeometryInstructions);
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
            return inputDeps;
        }
    }
}