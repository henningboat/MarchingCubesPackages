using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.Utils.Containers;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching
{
    [Serializable]
    public class GeometryGraphPropertyOverwrite
    {
        [SerializeField] private SerializableGUID _propertyGUID;
        [SerializeField] private float32 _value;
        [SerializeField] private GeometryPropertyValueProvider _providerObject;

        public GeometryGraphPropertyOverwrite(SerializableGUID guid)
        {
            _propertyGUID = guid;
        }

        public SerializableGUID PropertyGUID => _propertyGUID;

        public MonoBehaviour ProviderObject => _providerObject;

        public float32 Value
        {
            get => _value;
            set => _value = value;
        }

        public void SetProviderObject(GeometryPropertyValueProvider valueProvider)
        {
            _providerObject = valueProvider;
        }

        public void Reset(GeometryGraphProperty variable)
        {
            _providerObject = null;
            _value = variable.DefaultValue;
        }
    }

    public abstract class GeometryPropertyValueProvider : MonoBehaviour
    {
        public abstract float[] GetValue(GeometryGraphInstance graphInstance, GeometryPropertyType geometryPropertyType);
    }
}