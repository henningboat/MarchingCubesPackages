using System;
using System.Linq;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching
{
    [Serializable]
    public class NewGeometryGraphData
    {
        [SerializeField] private Hash128 _contentHash;
        [SerializeField] private GeometryGraphProperty _mainTransformation;

        [SerializeField] private float[] _valueBuffer;
        [SerializeField] private MathInstruction[] _mathInstructions;
        [SerializeField] private GeometryInstruction[] _geometryInstructions;
        [SerializeField] private GeometryGraphProperty[] _variables;


        public Hash128 ContentHash => _contentHash;
        public GeometryGraphProperty MainTransformation => _mainTransformation;
        public GeometryGraphProperty[] Variables => _variables;

        private NewGeometryGraphData()
        {
        }

        public static NewGeometryGraphData InitializeData(float[] valueBuffer, MathInstruction[] mathInstructions,
            GeometryInstruction[] geometryInstructions, Hash128 contentHash, GeometryGraphProperty mainTransformation,
            GeometryGraphProperty[] variables)
        {
            var data = new NewGeometryGraphData
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
        [SerializeField] private NewGeometryGraphData _geometryGraphData;

        public NewGeometryGraphData GeometryGraphData => _geometryGraphData;

#if UNITY_EDITOR
        public void Initialize(NewGeometryGraphData data)
        {
            _geometryGraphData = data;
        }
#endif
    }
}