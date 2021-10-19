using System;
using TerrainChunkEntitySystem;
using Unity.Jobs;
using UnityEngine;

namespace NonECSImplementation
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        private GeometryFieldData _geometryFieldData;
        private BuileRenderGraphSystem _buildRenderGraphSystem;
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
                _geometryFieldData.Initialize(1);
                _buildRenderGraphSystem.Initialize(_geometryFieldData);
                _updateDistanceFieldSystem.Initialize(_geometryFieldData);
                _updateMeshesSystem.Initialize(_geometryFieldData);
                _initialized = true;
            }

            var jobHandle = _buildRenderGraphSystem.Update();
            jobHandle =_updateDistanceFieldSystem.Update(jobHandle, _buildRenderGraphSystem.MainRenderGraph);
            _updateMeshesSystem.Update(jobHandle);
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

        public JobHandle Update()
        {
            //todo initialize in a nicer way
            _allGeometryGraphInstance = UnityEngine.Object.FindObjectsOfType<GeometryGraphInstance>();
            MainRenderGraph = _allGeometryGraphInstance[0].GraphData;
            return default;
        }

        public GeometryGraphData MainRenderGraph { get; private set; }
    }
}