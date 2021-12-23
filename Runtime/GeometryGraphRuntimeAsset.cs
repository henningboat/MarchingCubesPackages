using System;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
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
        [SerializeField] private Float4X4Value _mainTransformation;

        [SerializeField] private float[] _valueBuffer;
        [SerializeField] private MathInstruction[] _mathInstructions;
        [SerializeField] private GeometryInstruction[] _geometryInstructions;
        [SerializeField] private ExposedVariable[] _variables;


        public Hash128 ContentHash => _contentHash;

        public Float4X4Value MainTransformation => _mainTransformation;

        public ExposedVariable[] Variables => _variables;

        private NewGeometryGraphData()
        {
        }

        public static NewGeometryGraphData InitializeData(float[] valueBuffer, MathInstruction[] mathInstructions,
            GeometryInstruction[] geometryInstructions, Hash128 contentHash, Float4X4Value mainTransformation,
            ExposedVariable[] variables)
        {
            NewGeometryGraphData data = new NewGeometryGraphData();
            data._valueBuffer = valueBuffer.ToArray();
            data._mathInstructions = mathInstructions.ToArray();
            data._geometryInstructions = geometryInstructions.ToArray();
            data._contentHash = contentHash;
            data._mainTransformation = mainTransformation;
            data._variables = variables;
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

        public ExposedVariable GetIndexOfProperty(SerializableGUID overwritePropertyGuid)
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

    [Serializable]
    public class ExposedVariable:GeometryGraphProperty
    {
        [SerializeField] private SerializableGUID _id;
        [SerializeField] private GeometryPropertyType _type;
        [SerializeField] private float[] _defaultValue;
        [SerializeField] private int _indexInValueBuffer;
        [SerializeField] private string _name;

        public ExposedVariable(SerializableGUID id, GeometryPropertyType type, float[] defaultValue, int indexInValueBuffer, string name):base(indexInValueBuffer,type,name)
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