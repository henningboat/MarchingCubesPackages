using System;
using UnityEngine;

namespace NonECSImplementation
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        private GeometryFieldData _geometryFieldData;
        private UpdateMeshesSystem _updateMeshesSystem;
        
        private void Awake()
        {
        }

        public void Update()
        {
            if (_geometryFieldData == null)
            {
                Debug.Log("initializing");
                _geometryFieldData = new GeometryFieldData();
                _updateMeshesSystem = new UpdateMeshesSystem();
                _geometryFieldData.Initialize(1);
                _updateMeshesSystem.Initialize(_geometryFieldData);
            }
            _geometryFieldData.AddRandomVoxel();
            _updateMeshesSystem.Update();
        }

        private void OnDestroy()
        {
            Debug.Log("Disposing");
            _geometryFieldData.Dispose();
        }
    }
}