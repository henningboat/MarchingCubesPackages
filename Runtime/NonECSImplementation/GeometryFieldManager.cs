using System;
using UnityEngine;

namespace NonECSImplementation
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        private GeometryFieldData _geometryFieldData;
        private BuileRenderGraphSystem _buildRenderGraphSystem;
        private UpdateMeshesSystem _updateMeshesSystem;

        public void Update()
        {
            if (_geometryFieldData == null)
            {
                Debug.Log("initializing");
                _geometryFieldData = new GeometryFieldData();
                _updateMeshesSystem = new UpdateMeshesSystem();
                _buildRenderGraphSystem = new BuileRenderGraphSystem();
                _geometryFieldData.Initialize(1);
                _buildRenderGraphSystem.Initialize(_geometryFieldData);
                _updateMeshesSystem.Initialize(_geometryFieldData);
            }

            _buildRenderGraphSystem.Update();
            _updateMeshesSystem.Update();
        }

        private void OnDestroy()
        {
            Debug.Log("Disposing");
            _geometryFieldData.Dispose();
        }
    }

    internal class BuileRenderGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphInstance[] _allGeometryGraphInstance;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }

        public void Update()
        {
            //todo initialize in a nicer way
            _allGeometryGraphInstance = UnityEngine.Object.FindObjectsOfType<GeometryGraphInstance>();
            
        }
    }
}