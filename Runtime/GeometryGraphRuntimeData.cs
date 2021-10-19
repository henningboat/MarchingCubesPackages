using System.Collections.Generic;
using TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Hash128 = UnityEngine.Hash128;

public class GeometryGraphRuntimeData : ScriptableObject
{
    [SerializeField] private Hash128 _contentHash;
    
    [SerializeField] private float[] _valueBuffer;
    [SerializeField] private MathInstruction[] _mathInstructions;
    [SerializeField] private GeometryInstruction[] _geometryInstructions;

    public Hash128 ContentHash => _contentHash;

    public void InitializeData(List<float> valueBuffer, List<MathInstruction> mathInstructions,
        List<GeometryInstruction> geometryInstructions, Hash128 contentHash)
    {
        _valueBuffer = valueBuffer.ToArray();
        _mathInstructions = mathInstructions.ToArray();
        _geometryInstructions = geometryInstructions.ToArray();
        this._contentHash = contentHash;
    }

    public void AllocateNativeArrays(out NativeArray<float> values, out NativeArray<MathInstruction> mathInstructions, out NativeArray<GeometryInstruction> geometryInstructions)
    {
        values = MemoryHandler.CreateNativeArray<float>(_valueBuffer, Allocator.Persistent);
        mathInstructions = MemoryHandler.CreateNativeArray<MathInstruction>(_mathInstructions, Allocator.Persistent);
        geometryInstructions = MemoryHandler.CreateNativeArray<GeometryInstruction>(_geometryInstructions, Allocator.Persistent);
    }
}