using GeometrySystems.DistanceFieldGeneration;
using GeometrySystems.GeometryFieldSetup;
using GeometrySystems.MeshGenerationSystem;
using UnityEngine;

namespace NonECSImplementation
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        private GeometryFieldData _geometryFieldData;
        private BuileRenderGraphSystem _buildRenderGraphSystem;
        private DistanceFieldPrepassSystem _distanceFieldPrepass;
        private UpdateDistanceFieldSystem _updateDistanceFieldSystem;
        private UpdateMeshesSystem _updateMeshesSystem;
        private bool _initialized;

        public void Update()
        {
            if (!_initialized||_buildRenderGraphSystem==null)
            {
                Debug.Log("initializing");
                _geometryFieldData = new GeometryFieldData();
                _updateMeshesSystem = new UpdateMeshesSystem();
                _buildRenderGraphSystem = new BuileRenderGraphSystem();
                _updateDistanceFieldSystem = new UpdateDistanceFieldSystem();
                _distanceFieldPrepass = new DistanceFieldPrepassSystem();
                _geometryFieldData.Initialize(1);
                _buildRenderGraphSystem.Initialize(_geometryFieldData);
                _distanceFieldPrepass.Initialize(_geometryFieldData);
                _updateDistanceFieldSystem.Initialize(_geometryFieldData);
                _updateMeshesSystem.Initialize(_geometryFieldData);
                _initialized = true;
            }

            var jobHandle = _buildRenderGraphSystem.Update();
            jobHandle=_distanceFieldPrepass.Update(_buildRenderGraphSystem.MainRenderGraph,jobHandle);
            jobHandle =_updateDistanceFieldSystem.Update(jobHandle, _buildRenderGraphSystem.MainRenderGraph);
            _updateMeshesSystem.Update(jobHandle);
        }

        private void OnDestroy()
        {
            Debug.Log("Disposing");
            _geometryFieldData.Dispose();
        }
    }
}