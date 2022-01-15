using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [Serializable]
    public struct GeometryLayer
    {
        public bool Equals(GeometryLayer other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            return obj is GeometryLayer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        [SerializeField] private FixedString32Bytes _name;
        [SerializeField] private SerializableGUID _id;

        public GeometryLayer(FixedString32Bytes name, SerializableGUID id)
        {
            _name = name;
            _id = id;
        }

        public FixedString32Bytes Name => _name;

        public SerializableGUID ID => _id;

        public static bool operator ==(GeometryLayer a, GeometryLayer b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GeometryLayer a, GeometryLayer b)
        {
            return !(a == b);
        }
    }
}