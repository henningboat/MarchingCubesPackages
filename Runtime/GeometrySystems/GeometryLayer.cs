using System;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [CreateAssetMenu]
    [ExecuteInEditMode]
    public class GeometryLayer : ScriptableObject
    {
        [SerializeField] private bool _storeResults;
        [SerializeField] private SerializableGUID m_GeometryLayerID;

        public bool StoreResults => _storeResults;

        public SerializableGUID GeometryLayerID => m_GeometryLayerID;

        private void Awake()
        {
            m_GeometryLayerID = SerializableGUID.Generate();
        }
    }
}