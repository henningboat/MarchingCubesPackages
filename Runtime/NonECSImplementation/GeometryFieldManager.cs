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
            _geometryFieldData = new GeometryFieldData();
            _updateMeshesSystem = new UpdateMeshesSystem();
            _geometryFieldData.Initialize(1);
            _updateMeshesSystem.Initialize(_geometryFieldData);
        }

        public void Update()
        {
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