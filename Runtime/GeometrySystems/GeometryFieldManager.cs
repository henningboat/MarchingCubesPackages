﻿using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems
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

        public void Update()
        {
            var targetClusterCount = _clusterCounts;

            targetClusterCount = math.clamp(targetClusterCount, 1, 5);

            if (!_initialized || _buildRenderGraphSystem == null ||
                math.any(targetClusterCount != _geometryFieldData.ClusterCounts))
            {
                if (_initialized)
                {
                    _geometryFieldData.Dispose();
                    _updateMeshesSystem.Dispose();
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

            var jobHandle = new JobHandle();
            jobHandle = _prepareGraphsSystem.Update(jobHandle);
            jobHandle = _buildRenderGraphSystem.Update(jobHandle);
            jobHandle = _distanceFieldPrepass.Update(_buildRenderGraphSystem.MainRenderGraph, jobHandle);
            jobHandle = _updateDistanceFieldSystem.Update(jobHandle, _buildRenderGraphSystem.MainRenderGraph);
            _updateMeshesSystem.Update(jobHandle);
        }

        private void OnDestroy()
        {
            Debug.Log("Disposing");
            _geometryFieldData.Dispose();
            _updateMeshesSystem.Dispose();
        }
    }
}