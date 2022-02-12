﻿using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private List<GeometryLayerAsset> _geometryLayers = new();

        private GeometryLayerHandler _buildRenderGraphSystem;
        private GeometryFieldCollection _geometryFieldCollection;

        private List<IGeometryFieldReceiver> _geometryFieldReceivers = new();
        private bool _initialized;
        private PrepareGraphsSystem _prepareGraphsSystem;
        private JobHandle _receiverJobHandles;

        public void Update()
        {
#if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer) return;
#endif

            _geometryFieldReceivers = GetComponents<IGeometryFieldReceiver>().ToList();

            var storedGeometryLayers = new List<GeometryLayer>();

            storedGeometryLayers.AddRange(_geometryLayers.Select(asset =>
                asset != null ? asset.GeometryLayer : GeometryLayer.OutputLayer));
            storedGeometryLayers = storedGeometryLayers.Distinct().ToList();
            if (storedGeometryLayers.Count == 0) storedGeometryLayers.Add(GeometryLayer.OutputLayer);

            _geometryFieldCollection.InitializeIfDirty(storedGeometryLayers, _clusterCounts, out var didInitialize);

            _initialized = true;
            
            if (didInitialize)
                foreach (var geometryFieldReceiver in _geometryFieldReceivers)
                {
                    geometryFieldReceiver.Dispose();
                    geometryFieldReceiver.Initialize(
                        _geometryFieldCollection.GetFieldFromLayer(geometryFieldReceiver.RequestedLayer()));
                }

            var allGraphs = FindObjectsOfType<GeometryInstance>();
            var geometryGraphBuffers = new List<GeometryInstructionListBuffers>();
            foreach (var graph in allGraphs)
            {
                if (graph.enabled == false) continue;
                if (graph.TryInitializeAndGetBuffer(out var buffers)) geometryGraphBuffers.Add(buffers);
            }

            var jobHandle = new JobHandle();
            jobHandle = _prepareGraphsSystem.Update(jobHandle, geometryGraphBuffers);

            var allGeometryLayers = allGraphs.Select(instance => instance.TargetLayer).Distinct().ToList();
            jobHandle = _geometryFieldCollection.ScheduleJobs(jobHandle, geometryGraphBuffers, allGeometryLayers);

            _receiverJobHandles = default;

            jobHandle.Complete();

            foreach (var receiver in _geometryFieldReceivers)
                _receiverJobHandles = JobHandle.CombineDependencies(_receiverJobHandles,
                    receiver.ScheduleJobs(jobHandle,
                        _geometryFieldCollection.GetFieldFromLayer(receiver.RequestedLayer())));
        }

        private void LateUpdate()
        {
            _receiverJobHandles.Complete();
            foreach (var receiver in _geometryFieldReceivers)
                receiver.OnJobsFinished(_geometryFieldCollection.GetFieldFromLayer(receiver.RequestedLayer()));
        }

        private void OnEnable()
        {
            _initialized = false;
            _geometryFieldCollection = new GeometryFieldCollection();
            _prepareGraphsSystem = new PrepareGraphsSystem();
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                _geometryFieldCollection.Dispose();
                _initialized = false;
            }

            foreach (var receiver in _geometryFieldReceivers)
            {
                receiver.Dispose();
            }
        }
    }
}