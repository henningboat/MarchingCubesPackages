using System.Collections.Generic;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public class GeometryGraphInstance : GeometryInstanceBase
    {
        private static (ScriptableObject, GeometryGraphRuntimeData) _debugOverwrite;
        [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;


        public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;

        protected override GeometryGraphData GetGeometryGraphData()
        {
            return new GeometryGraphData(_geometryGraphRuntimeData);
        }


        public override void InitializeGraphDataIfNeeded()
        {
            var graphData = GraphData;
            GeometryGraphRuntimeData dataAssetToUse;

#if UNITY_EDITOR
            //find a better way to compare if they come from the same asset
            if (AssetDatabase.GetAssetPath(_geometryGraphRuntimeData) ==
                AssetDatabase.GetAssetPath(_debugOverwrite.Item1))
                dataAssetToUse = _debugOverwrite.Item2;
            else
#endif
                dataAssetToUse = _geometryGraphRuntimeData;

            if (graphData.ContentHash != dataAssetToUse.ContentHash)
            {
                if (graphData.GeometryInstructions.IsCreated)
                {
                    // graphData.Dispose();
                }

                GraphData = new GeometryGraphData(dataAssetToUse);
            }
        }

        private void UpdateOverwritesInValueBuffer()
        {
            foreach (var overwrite in _overwrites)
            {
                var variable = _geometryGraphRuntimeData.GetIndexOfProperty(overwrite.PropertyGUID);

                for (var i = 0; i < overwrite.Value.Length; i++)
                    GraphData.ValueBuffer.Write(overwrite.Value[i], variable.IndexInValueBuffer + i);
            }

            WriteTransformationToValueBuffer();
        }
        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return Overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }

        public override void UpdateOverwrites()
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