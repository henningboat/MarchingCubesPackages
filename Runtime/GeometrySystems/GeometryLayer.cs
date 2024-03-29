﻿using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [Serializable]
    public struct GeometryLayer
    {
        public static readonly GeometryLayer OutputLayer = new("OutputLayer", default, true, false);

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
        [SerializeField] private bool _stored;
        [SerializeField] private bool _clearEveryFrame;

        public GeometryLayer(FixedString32Bytes name, SerializableGUID id, bool storeResults, bool clearEveryFrame)
        {
            _name = name;
            _id = id;
            _stored = storeResults;
            _clearEveryFrame = clearEveryFrame;
        }

        public FixedString32Bytes Name => _name;

        public SerializableGUID ID => _id;

        public bool Stored => _stored;

        public bool ClearEveryFrame => _clearEveryFrame;

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