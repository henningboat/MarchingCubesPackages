using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Output.GeometryFieldSFDOutputSystem
{
    public class GeometryFieldSFDOutput : MonoBehaviour, IGeometryFieldReceiver
    {
        [SerializeField] private GeometryLayerAsset _geometryLayerAsset;
        [SerializeField] private Texture3D _sdf;
        private NativeList<int> _chunksModifiedThisFrame;
        private NativeArray<float> _sdfTextureData;
        private int3 _voxelCounts;

        public Texture3D SDF => _sdf;
        public Vector3 SDFPosition { get; private set; }
        public Vector3 SDFScale { get; private set; }

        public void Dispose()
        {
            if (Application.isPlaying)
                Destroy(_sdf);
            else
                DestroyImmediate(_sdf);
        }

        public GeometryLayer RequestedLayer()
        {
            return _geometryLayerAsset != null ? _geometryLayerAsset.GeometryLayer : GeometryLayer.OutputLayer;
        }

        public JobHandle ScheduleJobs(JobHandle dependencies, GeometryFieldData requestedField,
            NativeList<int> chunksModifiedThisFrame)
        {
            _chunksModifiedThisFrame = chunksModifiedThisFrame;

            SDFScale = (float3) requestedField.ClusterCounts * Constants.clusterLength;
            SDFPosition = SDFScale * 0.5f;

            return default;
        }

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _voxelCounts = geometryFieldData.VoxelCounts;
            _sdf = new Texture3D(_voxelCounts.x, _voxelCounts.y, _voxelCounts.z, TextureFormat.RFloat, false);
        }

        public void OnJobsFinished(GeometryFieldData geometryFieldData)
        {
            _sdfTextureData = _sdf.GetPixelData<float>(0);
            var job = new WriteSDFData
                {VoxelCounts = _voxelCounts, GeometryFieldData = geometryFieldData, SDFData = _sdfTextureData};

            job.Schedule().Complete();

            _sdfTextureData.Dispose();
            _sdf.Apply();
        }

        [BurstCompile]
        private struct WriteSDFData : IJob
        {
            [WriteOnly] public NativeArray<float> SDFData;
            [ReadOnly] public GeometryFieldData GeometryFieldData;
            public int3 VoxelCounts;

            public void Execute()
            {
                for (var i = 0; i < SDFData.Length; i++)
                {
                    var voxelPosition = DistanceFieldGeneration.Utils.IndexToPositionWS(i, VoxelCounts);
                    SDFData[i] = GeometryFieldData.GetSingleDistance(voxelPosition) / 64f;
                }
            }
        }
    }
}