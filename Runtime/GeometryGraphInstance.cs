using System.Collections.Generic;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEditor;
using UnityEngine;

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public class GeometryGraphInstance : MonoBehaviour
    {
        private static (ScriptableObject, GeometryGraphRuntimeData) _debugOverwrite;
        [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;
        private GeometryGraphData _graphData;

        internal GeometryGraphData GetGraphData()
        {
                var graphData = _graphData;
                GeometryGraphRuntimeData dataAssetToUse;
                
                //find a better way to compare if they come from the same asset
                if (AssetDatabase.GetAssetPath(_geometryGraphRuntimeData) == AssetDatabase.GetAssetPath(_debugOverwrite.Item1))
                    dataAssetToUse = _debugOverwrite.Item2;
                else
                    dataAssetToUse = _geometryGraphRuntimeData;

                if (graphData.ContentHash != dataAssetToUse.ContentHash)
                {
                    if (graphData.GeometryInstructions.IsCreated)
                    {
                        //todo deallocating this creates a memory leak that I don't really understand.
                        //Needs to be fixed
                    //    graphData.Dispose();
                    }

                    _graphData = new GeometryGraphData(dataAssetToUse);
                }

                return graphData;
        }

        public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;


        private void OnEnable()
        {
            _graphData = new GeometryGraphData(_geometryGraphRuntimeData);
        }

        private void OnDisable()
        {
            _graphData.Dispose();
        }

        private void UpdateOverwritesInValueBuffer()
        {
            //apply main transformation
            _graphData.ValueBuffer.Write(transform.worldToLocalMatrix,
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