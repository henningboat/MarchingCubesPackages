﻿using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.Serialization;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [CreateAssetMenu]
    public class GeometryLayerAsset : ScriptableObject
    {
        [SerializeField] private bool _storeResults;

        [FormerlySerializedAs("m_GeometryLayerID")] [SerializeField]
        private SerializableGUID _geometryLayerID;

        [SerializeField] private bool _clearOnUpdate = true;

        public bool StoreResults => _storeResults;

        public GeometryLayer GeometryLayer => new(name, _geometryLayerID, StoreResults, _clearOnUpdate);

        private void Awake()
        {
            if (!_geometryLayerID.Valid) _geometryLayerID = SerializableGUID.Generate();
        }
    }
}