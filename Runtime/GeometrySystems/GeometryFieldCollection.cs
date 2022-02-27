using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.Output.GeometryFieldMeshRendererSystem;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    public class GeometryFieldCollection
    {
        private GeometryLayerHandler[] _geometryLayerHandlers;
        private Dictionary<SerializableGUID, List<GeometryInstructionListBuffers>> _geometryPerLayer;
        private List<GeometryLayer> _storedGeometryLayers;
        public GeometryFieldData GetOutputFieldData => _geometryLayerHandlers.Last().GeometryFieldData;

        public void InitializeIfDirty(List<GeometryLayer> storedGeometryLayers, int3 size, out bool didInitialize)
        {
            if (!CheckDirty(storedGeometryLayers, size))
            {
                didInitialize = false;
                return;
            }

            ;

            Dispose();

            _storedGeometryLayers = storedGeometryLayers;
            _geometryLayerHandlers = new GeometryLayerHandler[storedGeometryLayers.Count];

            for (var i = 0; i < storedGeometryLayers.Count; i++)
                _geometryLayerHandlers[i] = new GeometryLayerHandler(size, storedGeometryLayers[i]);

            didInitialize = true;
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
            List<GeometryInstructionListBuffers> geometryInstructionListBuffersList,
            List<GeometryLayer> allGeometryLayers, bool forceClear, AssetDataStorage assetDataStorage)
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
                jobHandle = geometryFieldHandler.Update(jobHandle, _geometryPerLayer, _storedGeometryLayers,
                    allGeometryLayers, _geometryLayerHandlers.ToList(),forceClear, assetDataStorage);

            return jobHandle;
        }

        public GeometryFieldData LayerByIndex(int i)
        {
            return _geometryLayerHandlers[i].GeometryFieldData;
        }

        public GeometryLayerHandler GetFieldFromLayer(GeometryLayer requestedLayer)
        {
            return _geometryLayerHandlers.First(handler => handler.GeometryLayer == requestedLayer);
        }
    }
}