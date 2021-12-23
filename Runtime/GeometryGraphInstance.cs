using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
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
        private static (ScriptableObject, GeometryGraphRuntimeAsset) _debugOverwrite;
        [SerializeField] private GeometryGraphRuntimeAsset _geometryGraphRuntimeData;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;


        public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;

        protected override NewGeometryGraphData GetGeometryGraphData()
        {
            throw new NotImplementedException();
            //n new GeometryGraphData(_geometryGraphRuntimeData.GeometryGraphData);
        }


        public override void InitializeGraphDataIfNeeded()
        {   
            var graphData = GraphBuffers;
            NewGeometryGraphData dataAssetToUse;

#if UNITY_EDITOR
            //find a better way to compare if they come from the same asset
            if (AssetDatabase.GetAssetPath(_geometryGraphRuntimeData) ==
                AssetDatabase.GetAssetPath(_debugOverwrite.Item1))
                dataAssetToUse = _debugOverwrite.Item2.GeometryGraphData;
            else
#endif
                dataAssetToUse = _geometryGraphRuntimeData.GeometryGraphData;

            if (graphData.ContentHash != dataAssetToUse.ContentHash)
            {
                if (graphData.GeometryInstructions.IsCreated)
                {
                    // graphData.Dispose();
                }

                GraphBuffers = new GeometryGraphBuffers(GetGeometryGraphData());
            }  
            
            TransformationValue = new GeometryGraphConstantProperty(dataAssetToUse.MainTransformation.Index,new Matrix4x4(),GeometryPropertyType.Float4X4);

        }

        private void UpdateOverwritesInValueBuffer()
        {
            foreach (var overwrite in _overwrites)
            {
                var variable = _geometryGraphRuntimeData.GeometryGraphData.GetIndexOfProperty(overwrite.PropertyGUID);

                for (var i = 0; i < overwrite.Value.Length; i++)
                    GraphBuffers.ValueBuffer.Write(overwrite.Value[i], variable.IndexInValueBuffer + i);
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

        public static void SetDebugOverwrite(ScriptableObject graphModelAssetModel, GeometryGraphRuntimeAsset data)
        {
            _debugOverwrite = (graphModelAssetModel, data);
        }

        public static void ClearDebugOverwrite()
        {
            _debugOverwrite = default;
        }
    }
}