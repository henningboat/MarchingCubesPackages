using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        [SerializeField] private int3 _clusterCounts = new(1, 1, 1);
        [SerializeField] private List<GeometryLayerAsset> _additionalGeometryLayers = new();

        private GeometryLayerHandler _buildRenderGraphSystem;
        private bool _initialized;
        private PrepareGraphsSystem _prepareGraphsSystem;
        private UpdateMeshesSystem _updateMeshesSystem;
        private GeometryFieldCollection _geometryFieldCollection;

        [SerializeField] private bool _outputOtherLayer;
        

        private void OnEnable()
        {
            _initialized = false;
            _geometryFieldCollection = new GeometryFieldCollection();
            _prepareGraphsSystem = new PrepareGraphsSystem();
            _updateMeshesSystem = new UpdateMeshesSystem();
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer) return;
#endif

            var allGeometryLayers = new List<GeometryLayer>();
            foreach (var additionalGeometryLayer in _additionalGeometryLayers)
                allGeometryLayers.Add(additionalGeometryLayer.GeometryLayer);
            allGeometryLayers.Add(GeometryLayer.OutputLayer);

            _geometryFieldCollection.InitializeIfDirty(allGeometryLayers, _clusterCounts);

            var allGraphs = FindObjectsOfType<GeometryInstance>();
            var geometryGraphBuffers = new List<GeometryInstructionListBuffers>();
            foreach (var graph in allGraphs)
            {
                if (graph.enabled == false) continue;
                if (graph.TryInitializeAndGetBuffer(out var buffers)) geometryGraphBuffers.Add(buffers);
            }

            var jobHandle = new JobHandle();
            jobHandle = _prepareGraphsSystem.Update(jobHandle, geometryGraphBuffers);

            jobHandle = _geometryFieldCollection.ScheduleJobs(jobHandle, geometryGraphBuffers);

            if (_outputOtherLayer)
            {
                _updateMeshesSystem.Update(jobHandle, _geometryFieldCollection.LayerByIndex(0));
            }
            else
            {
                _updateMeshesSystem.Update(jobHandle, _geometryFieldCollection.GetOutputFieldData);
            }
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                _updateMeshesSystem.Dispose();


                _initialized = false;
            }
        }
    }
}