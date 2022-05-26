using UnityEngine;
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

        [SerializeField] private Material _material;

        [SerializeField] private bool _render;
        
        public bool StoreResults => _storeResults;

        public GeometryLayer GeometryLayer => new(name, _geometryLayerID, StoreResults, _clearOnUpdate);

        public Material material => _material;

        public bool render => _render;

        private void Awake()
        {
            if (!_geometryLayerID.Valid) _geometryLayerID = SerializableGUID.Generate();
        }
    }
}