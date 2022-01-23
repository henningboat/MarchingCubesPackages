using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    public class GeometryFieldCollection
    {
        private GeometryLayerHandler[] _geometryLayerHandlers;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;
        private List<GeometryLayer> _geometryLayers;
        public GeometryFieldData GetOutputFieldData => _geometryLayerHandlers.Last().GeometryFieldData;

        public void InitializeIfDirty(List<GeometryLayer> geometryLayers, int3 size)
        {
            if (!CheckDirty(geometryLayers, size)) return;

            Dispose();

            _geometryLayers = geometryLayers;
            _geometryLayerHandlers = new GeometryLayerHandler[geometryLayers.Count];
            
            for (var i = 0; i < geometryLayers.Count; i++)
                _geometryLayerHandlers[i] = new GeometryLayerHandler(size, geometryLayers[i]);
        }

        public void Dispose()
        {
            if (_geometryLayerHandlers != null)
                foreach (var data in _geometryLayerHandlers)
                    data.Dispose();
        }

        private bool CheckDirty(List<GeometryLayer> additionalStoredLayers, int3 clusterCounts)
        {
            if (_geometryLayerHandlers == null) return true;

            if (math.any(clusterCounts != _geometryLayerHandlers[0].ClusterCounts)) return true;

            for (var i = 0; i < additionalStoredLayers.Count; i++)
                if (!Equals(additionalStoredLayers[i], _geometryLayerHandlers[i].GeometryLayer))
                    return true;

            return false;
        }

        public JobHandle ScheduleJobs(JobHandle jobHandle,
            List<GeometryInstructionListBuffers> geometryInstructionListBuffersList)
        {
            _geometryPerLayer = new Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>>();

            foreach (var geometryInstructionList in geometryInstructionListBuffersList)
            {
                if (!_geometryPerLayer.ContainsKey(geometryInstructionList.TargetLayer.ID))
                    _geometryPerLayer.Add(geometryInstructionList.TargetLayer.ID,
                        new List<GeometryInstructionListBuffers>());

                _geometryPerLayer[geometryInstructionList.TargetLayer.ID].Add(geometryInstructionList);
            }

            foreach (var geometryFieldHandler in _geometryLayerHandlers)
            {
               jobHandle = geometryFieldHandler.Update(jobHandle, _geometryPerLayer, _geometryLayers);
            }

            return jobHandle;
        }

        public GeometryFieldData LayerByIndex(int i)
        {
            return _geometryLayerHandlers[i].GeometryFieldData;
        }
    }
}