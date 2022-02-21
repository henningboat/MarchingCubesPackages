using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using henningboat.CubeMarching.Runtime.Output.GeometryFieldMeshRendererSystem;
using Unity.Collections;
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
        [SerializeField][Tooltip("Always overwrite layers, even if they have ")] private bool _alwaysClearInEditMode=true;
        
        
        private GeometryLayerHandler _buildRenderGraphSystem;
        private NativeList<int> _chhunksToUploadToGPU;
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
                        _geometryFieldCollection.GetFieldFromLayer(geometryFieldReceiver.RequestedLayer())
                            .GeometryFieldData);
                }

            var allGraphs = FindObjectsOfType<GeometryInstance>().OrderBy(instance => instance.Order);
            var geometryGraphBuffers = new List<GeometryInstructionListBuffers>();
            foreach (var graph in allGraphs)
            {
                if (graph.enabled == false) continue;
                if (graph.TryInitializeAndGetBuffer(out var buffers)) geometryGraphBuffers.Add(buffers);
            }

            var jobHandle = new JobHandle();
            jobHandle = _prepareGraphsSystem.Update(jobHandle, geometryGraphBuffers);

            var allGeometryLayers = allGraphs.Select(instance => instance.TargetLayer).Distinct().ToList();
            
            var forceClear = Application.isPlaying==false && _alwaysClearInEditMode;
            jobHandle = _geometryFieldCollection.ScheduleJobs(jobHandle, geometryGraphBuffers, allGeometryLayers, forceClear);

            _receiverJobHandles = default;

            jobHandle.Complete();


            foreach (var receiver in _geometryFieldReceivers)
            {
                var geometryFieldData = _geometryFieldCollection.GetFieldFromLayer(receiver.RequestedLayer());
                jobHandle = receiver.ScheduleJobs(jobHandle, geometryFieldData.GeometryFieldData,
                    geometryFieldData.ChunksUpdatedThisFrame);
            }

            _receiverJobHandles = jobHandle;
        }

        private void LateUpdate()
        {
            _receiverJobHandles.Complete();
            foreach (var receiver in _geometryFieldReceivers)
                receiver.OnJobsFinished(_geometryFieldCollection.GetFieldFromLayer(receiver.RequestedLayer())
                    .GeometryFieldData);
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

            foreach (var receiver in _geometryFieldReceivers) receiver.Dispose();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube((float3) _clusterCounts * 0.5f * Constants.clusterLength,
                (float3) _clusterCounts * Constants.clusterLength * Vector3.one);
        }
    }
}