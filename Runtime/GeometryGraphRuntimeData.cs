using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching
{
    public class GeometryGraphRuntimeData : ScriptableObject
    {
        [SerializeField] private Hash128 _contentHash;
        [SerializeField] private Float4X4Value _mainTransformation;

        [SerializeField] private float[] _valueBuffer;
        [SerializeField] private MathInstruction[] _mathInstructions;
        [SerializeField] private GeometryInstruction[] _geometryInstructions;
        [SerializeField] private ExposedVariable[] _variables;
        
        
        public Hash128 ContentHash => _contentHash;

        public Float4X4Value MainTransformation => _mainTransformation;

        public ExposedVariable[] Variables => _variables;

        public void InitializeData(List<float> valueBuffer, List<MathInstruction> mathInstructions,
            List<GeometryInstruction> geometryInstructions, Hash128 contentHash, Float4X4Value mainTransformation, ExposedVariable[] variables)
        {
            _valueBuffer = valueBuffer.ToArray();
            _mathInstructions = mathInstructions.ToArray();
            _geometryInstructions = geometryInstructions.ToArray();
            _contentHash = contentHash;
            _mainTransformation = mainTransformation;
            _variables = variables;
        }

        public void AllocateNativeArrays(out NativeArray<float> values,
            out NativeArray<MathInstruction> mathInstructions,
            out NativeArray<GeometryInstruction> geometryInstructions)
        {
            values = new NativeArray<float>(_valueBuffer, Allocator.Persistent);
            mathInstructions = new NativeArray<MathInstruction>(_mathInstructions, Allocator.Persistent);
            geometryInstructions = new NativeArray<GeometryInstruction>(_geometryInstructions, Allocator.Persistent);
        }

        public ExposedVariable GetIndexOfProperty(SerializableGUID overwritePropertyGuid)
        {
            return _variables.FirstOrDefault(variable => variable.ID == overwritePropertyGuid);
        }
    }
    
    [Serializable]
    public class ExposedVariable
    {
        [SerializeField] private SerializableGUID _id;
        [SerializeField] private GeometryPropertyType _type;
        [SerializeField] private float[] _defaultValue;
        [SerializeField] private int _indexInValueBuffer;
        [SerializeField] private string _name;

        public ExposedVariable(SerializableGUID id, GeometryPropertyType type, float[] defaultValue, int indexInValueBuffer, string name)
        {
            _id = id;
            _type = type;
            _defaultValue = defaultValue;
            _indexInValueBuffer = indexInValueBuffer;
            _name = name;
        }

        public SerializableGUID ID => _id;
        public GeometryPropertyType Type => _type;
        public float[] DefaultValue => _defaultValue;
        public int IndexInValueBuffer => _indexInValueBuffer;

        public string Name => _name;
    }
}