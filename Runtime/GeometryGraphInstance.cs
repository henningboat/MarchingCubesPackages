using System.Collections.Generic;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public class GeometryGraphInstance : MonoBehaviour
    {
        [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;
        private GeometryGraphData _graphData;

        internal GeometryGraphData GraphData
        {
            get
            {
                if (_graphData.ContentHash != _geometryGraphRuntimeData.ContentHash)
                {
                    if (_graphData.GeometryInstructions.IsCreated)
                    {
                        _graphData.Dispose();
                    }

                    _graphData = new GeometryGraphData(_geometryGraphRuntimeData);
                }

                return _graphData;
            }
        }

        private void UpdateOverwritesInValueBuffer()
        {
            //apply main transformation
            _graphData.ValueBuffer.Write(transform.worldToLocalMatrix,_geometryGraphRuntimeData.MainTransformation.Index);
        }

        public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;

        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return Overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }


        private void OnEnable()
        {
            _graphData = new GeometryGraphData(_geometryGraphRuntimeData);
        }

        private void OnDisable()
        {
            _graphData.Dispose();
        }

        public void UpdateOverwrites()
        { 
            UpdateOverwritesInValueBuffer();
        }
    }
}