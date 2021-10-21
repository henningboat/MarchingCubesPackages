using System.Collections.Generic;
using GeometryComponents;
using GeometrySystems.GeometryMath;
using TerrainChunkEntitySystem;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Serialization;
using Hash128 = UnityEngine.Hash128;

public class GeometryGraphRuntimeData : ScriptableObject
{
    [SerializeField] private Hash128 _contentHash;
    [SerializeField] private Float4X4Value _mainTransformation;
    
    [SerializeField] private float[] _valueBuffer;
    [SerializeField] private MathInstruction[] _mathInstructions;
    [SerializeField] private GeometryInstruction[] _geometryInstructions;

    public Hash128 ContentHash => _contentHash;

    public Float4X4Value MainTransformation => _mainTransformation;

    public void InitializeData(List<float> valueBuffer, List<MathInstruction> mathInstructions,
        List<GeometryInstruction> geometryInstructions, Hash128 contentHash, Float4X4Value mainTransformation)
    {
        _valueBuffer = valueBuffer.ToArray();
        _mathInstructions = mathInstructions.ToArray();
        _geometryInstructions = geometryInstructions.ToArray();
        _contentHash = contentHash;
        _mainTransformation = mainTransformation;
    }

    public void AllocateNativeArrays(out NativeArray<float> values, out NativeArray<MathInstruction> mathInstructions, out NativeArray<GeometryInstruction> geometryInstructions)
    {
        values = MemoryHandler.CreateNativeArray<float>(_valueBuffer, Allocator.Persistent);
        mathInstructions = MemoryHandler.CreateNativeArray<MathInstruction>(_mathInstructions, Allocator.Persistent);
        geometryInstructions = MemoryHandler.CreateNativeArray<GeometryInstruction>(_geometryInstructions, Allocator.Persistent);
    }
}