using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        private GeometryFieldData _geometryFieldData;
        private PrepareGraphsSystem _prepareGraphsSystem;
        private BuildMainGraphSystem _buildRenderGraphSystem;
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
                _prepareGraphsSystem = new PrepareGraphsSystem();
                _buildRenderGraphSystem = new BuildMainGraphSystem();
                _updateDistanceFieldSystem = new UpdateDistanceFieldSystem();
                _distanceFieldPrepass = new DistanceFieldPrepassSystem();
                
                _geometryFieldData.Initialize(1);
                _prepareGraphsSystem.Initialize();
                _buildRenderGraphSystem.Initialize(_geometryFieldData);
                _distanceFieldPrepass.Initialize(_geometryFieldData);
                _updateDistanceFieldSystem.Initialize(_geometryFieldData);
                _updateMeshesSystem.Initialize(_geometryFieldData);
                _initialized = true;
            }

            JobHandle jobHandle = new JobHandle();
             jobHandle = _prepareGraphsSystem.Update(jobHandle);
            jobHandle = _buildRenderGraphSystem.Update(jobHandle);
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