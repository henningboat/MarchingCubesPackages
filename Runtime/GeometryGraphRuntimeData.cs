using System.Collections.Generic;
using TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GeometryGraphRuntimeData : ScriptableObject
{
    [SerializeField] private float[] _valueBuffer;
    [SerializeField] private MathInstruction[] _mathInstructions;
    [SerializeField] private GeometryInstruction[] _geometryInstructions;

    public void InitializeData(List<float> valueBuffer, List<MathInstruction> mathInstructions, List<GeometryInstruction> geometryInstructions)
    {
        _valueBuffer = valueBuffer.ToArray();
        _mathInstructions = mathInstructions.ToArray();
        _geometryInstructions = geometryInstructions.ToArray();
    }

    public void AllocateNativeArrays(out NativeArray<float> values, out NativeArray<MathInstruction> mathInstructions, NativeArray<GeometryInstruction> geometryInstructions)
    {
        values = MemoryHandler.CreateNativeArray<float>(_valueBuffer, Allocator.Persistent);
        mathInstructions = MemoryHandler.CreateNativeArray<>(_mathInstructions, Allocator.Persistent);
        geometryInstructions = MemoryHandler.CreateNativeArray<>(_geometryInstructions, Allocator.Persistent);
    }
}