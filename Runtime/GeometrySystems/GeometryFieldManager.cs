using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [ExecuteInEditMode]
    public class GeometryFieldManager : MonoBehaviour
    {
        [SerializeField] private int3 _clusterCounts = new(1, 1, 1);
        private BuildMainGraphSystem _buildRenderGraphSystem;
        private DistanceFieldPrepassSystem _distanceFieldPrepass;
        private GeometryFieldData _geometryFieldData;
        private bool _initialized;
        private PrepareGraphsSystem _prepareGraphsSystem;
        private UpdateDistanceFieldSystem _updateDistanceFieldSystem;
        private UpdateMeshesSystem _updateMeshesSystem;

        private void OnEnable()
        {
            _initialized = false;
        }

        public void Update()
        {
            #if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
            {
                return;
            }
            #endif

            var targetClusterCount = _clusterCounts;

            targetClusterCount = math.clamp(targetClusterCount, 1, 5);

            if (!_initialized || _buildRenderGraphSystem == null ||
                math.any(targetClusterCount != _geometryFieldData.ClusterCounts))
            {
                if (_initialized)
                {
                    _geometryFieldData.Dispose();
                    _updateMeshesSystem.Dispose();
                    if (_buildRenderGraphSystem != null)
                    {
                        _buildRenderGraphSystem.Dispose();
                    }
                }
                
                _geometryFieldData = new GeometryFieldData();
                _updateMeshesSystem = new UpdateMeshesSystem();
                _prepareGraphsSystem = new PrepareGraphsSystem();
                _buildRenderGraphSystem = new BuildMainGraphSystem();
                _updateDistanceFieldSystem = new UpdateDistanceFieldSystem();
                _distanceFieldPrepass = new DistanceFieldPrepassSystem();

                _geometryFieldData.Initialize(targetClusterCount);
                _prepareGraphsSystem.Initialize();
                _buildRenderGraphSystem.Initialize(_geometryFieldData);
                _distanceFieldPrepass.Initialize(_geometryFieldData);
                _updateDistanceFieldSystem.Initialize(_geometryFieldData);
                _updateMeshesSystem.Initialize(_geometryFieldData);
                _initialized = true;
            }
            
            var allGraphs = Object.FindObjectsOfType<GeometryInstance>();
            List<GeometryGraphBuffers> geometryGraphBuffers = new List<GeometryGraphBuffers>();
            foreach (var graph in allGraphs)
            {
                if (graph.enabled == false)
                {
                    continue;
                }
                if (graph.TryInitializeAndGetBuffer(out GeometryGraphBuffers buffers))
                {
                    geometryGraphBuffers.Add(buffers);
                }
            }

            var jobHandle = new JobHandle();
            jobHandle = _prepareGraphsSystem.Update(jobHandle,geometryGraphBuffers);
            jobHandle = _buildRenderGraphSystem.Update(jobHandle,geometryGraphBuffers);
            jobHandle = _distanceFieldPrepass.Update(_buildRenderGraphSystem.MainRenderGraph, jobHandle);
            jobHandle = _updateDistanceFieldSystem.Update(jobHandle, _buildRenderGraphSystem.MainRenderGraph);
            _updateMeshesSystem.Update(jobHandle);
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                _geometryFieldData.Dispose();
                _updateMeshesSystem.Dispose();
                _buildRenderGraphSystem.Dispose();


                _initialized = false;
            }
        }
    }
}