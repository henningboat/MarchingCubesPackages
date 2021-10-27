using System.Collections.Generic;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using UnityEngine;

namespace henningboat.CubeMarching
{
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

        public void AllocateNativeArrays(out NativeArray<float> values,
            out NativeArray<MathInstruction> mathInstructions,
            out NativeArray<GeometryInstruction> geometryInstructions)
        {
            values = new NativeArray<float>(_valueBuffer, Allocator.Persistent);
            mathInstructions = new NativeArray<MathInstruction>(_mathInstructions, Allocator.Persistent);
            geometryInstructions = new NativeArray<GeometryInstruction>(_geometryInstructions, Allocator.Persistent);
        }
    }
}