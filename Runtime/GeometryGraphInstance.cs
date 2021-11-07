using System.Collections.Generic;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public class GeometryGraphInstance : MonoBehaviour
    {
        private static (ScriptableObject, GeometryGraphRuntimeData) _debugOverwrite;
        [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;

        public void InitializeGraphDataIfNeeded()
        {
            var graphData = GraphData;
            GeometryGraphRuntimeData dataAssetToUse;

#if UNITY_EDITOR
            //find a better way to compare if they come from the same asset
            if (AssetDatabase.GetAssetPath(_geometryGraphRuntimeData) ==
                AssetDatabase.GetAssetPath(_debugOverwrite.Item1))
            {
                dataAssetToUse = _debugOverwrite.Item2;
            }
            else
#endif
            {
                dataAssetToUse = _geometryGraphRuntimeData;
            }

            if (graphData.ContentHash != dataAssetToUse.ContentHash)
            {
                if (graphData.GeometryInstructions.IsCreated)
                {
                    // graphData.Dispose();
                }

                GraphData = new GeometryGraphData(dataAssetToUse);
            }
        }

        public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;

        public GeometryGraphData GraphData { get; set; }


        private void OnEnable()
        {
            GraphData = new GeometryGraphData(_geometryGraphRuntimeData);
        }

        private void OnDisable()
        {
            GraphData.Dispose();
        }

        private void UpdateOverwritesInValueBuffer()
        {
            //apply main transformation
            GraphData.ValueBuffer.Write(transform.worldToLocalMatrix,
                _geometryGraphRuntimeData.MainTransformation.Index);
        }

        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return Overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }

        public void UpdateOverwrites()
        {
            UpdateOverwritesInValueBuffer();
        }

        public static void SetDebugOverwrite(ScriptableObject graphModelAssetModel, GeometryGraphRuntimeData data)
        {
            _debugOverwrite = (graphModelAssetModel, data);
        }

        public static void ClearDebugOverwrite()
        {
            _debugOverwrite = default;
        }
    }
}