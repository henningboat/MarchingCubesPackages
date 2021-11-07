using System;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching
{
    [Serializable]
    public class GeometryGraphPropertyOverwrite
    {
        [SerializeField] private SerializableGUID _propertyGUID;
        [SerializeField] private float[] _value;
        [SerializeField] private GeometryPropertyValueProvider _providerObject;

        public GeometryGraphPropertyOverwrite(SerializableGUID guid)
        {
            _propertyGUID = guid;
        }

        public SerializableGUID PropertyGUID => _propertyGUID;

        public MonoBehaviour ProviderObject => _providerObject;

        public float[] Value => _value;

        public void SetValueCapacity(int length)
        {
            if (_value == null || _value.Length != length)
            {
                _value = new float[length];
            }
        }

        public void SetProviderObject(GeometryPropertyValueProvider valueProvider)
        {
            _providerObject = valueProvider;
        }

        public void Reset(ExposedVariable variable)
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