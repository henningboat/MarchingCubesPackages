using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.Serialization;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [CreateAssetMenu]
    public class GeometryLayerAsset : ScriptableObject
    {
        [SerializeField] private int _order;
        
        [SerializeField] private bool _storeResults;

        [FormerlySerializedAs("m_GeometryLayerID")] [SerializeField]
        private SerializableGUID _geometryLayerID;

        [SerializeField] private bool _clearOnUpdate = true;

        [SerializeField] private Material[] _materials;

        [SerializeField] private bool _render;
        
        public bool StoreResults => _storeResults;

        public GeometryLayer GeometryLayer => new(name, _geometryLayerID, StoreResults, _clearOnUpdate);

        public Material[] materials => _materials;

        public bool render => _render;

        public SerializableGUID geometryLayerID => _geometryLayerID;

        public int order => _order;

        public bool clearOnUpdate => _clearOnUpdate;

        private void Awake()
        {
            if (!_geometryLayerID.Valid) _geometryLayerID = SerializableGUID.Generate();
        }
    }
}