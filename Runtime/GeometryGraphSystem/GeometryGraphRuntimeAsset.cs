using System;
using System.Linq;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using Unity.Collections;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometryGraphSystem
{
    [Serializable]
    public class GeometryInstructionList
    {
        [SerializeField] private Hash128 _contentHash;
        [SerializeField] private GeometryGraphProperty _mainTransformation;

        [SerializeField] private float[] _valueBuffer;
        [SerializeField] private MathInstruction[] _mathInstructions;
        [SerializeField] private GeometryInstruction[] _geometryInstructions;
        [SerializeField] private GeometryGraphProperty[] _variables = new GeometryGraphProperty[0];


        public Hash128 ContentHash => _contentHash;
        public GeometryGraphProperty MainTransformation => _mainTransformation;
        public GeometryGraphProperty[] Variables => _variables;

        private GeometryInstructionList()
        {
        }

        public static GeometryInstructionList InitializeData(float[] valueBuffer, MathInstruction[] mathInstructions,
            GeometryInstruction[] geometryInstructions, Hash128 contentHash, GeometryGraphProperty mainTransformation,
            GeometryGraphProperty[] variables)
        {
            var data = new GeometryInstructionList
            {
                _valueBuffer = valueBuffer.ToArray(),
                _mathInstructions = mathInstructions.ToArray(),
                _geometryInstructions = geometryInstructions.ToArray(),
                _contentHash = contentHash,
                _mainTransformation = mainTransformation,
                _variables = variables
            };
            return data;
        }

        public void AllocateNativeArrays(out NativeArray<float> values,
            out NativeArray<MathInstruction> mathInstructions,
            out NativeArray<GeometryInstruction> geometryInstructions)
        {
            values = new NativeArray<float>(_valueBuffer, Allocator.Persistent);
            mathInstructions = new NativeArray<MathInstruction>(_mathInstructions, Allocator.Persistent);
            geometryInstructions =
                new NativeArray<GeometryInstruction>(_geometryInstructions, Allocator.Persistent);
        }

        public GeometryGraphProperty GetIndexOfProperty(SerializableGUID overwritePropertyGuid)
        {
            return _variables.FirstOrDefault(variable => variable.ID == overwritePropertyGuid);
        }
    }

    public class GeometryGraphRuntimeAsset : ScriptableObject
    {
        [SerializeField] private GeometryInstructionList geometryInstructionList;

        public GeometryInstructionList GeometryInstructionList => geometryInstructionList;

#if UNITY_EDITOR
        public void Initialize(GeometryInstructionList data)
        {
            geometryInstructionList = data;
        }
#endif
    }
}