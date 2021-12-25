using System;
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
        private static (ScriptableObject, GeometryGraphRuntimeAsset) _debugOverwrite;
        [SerializeField] protected GeometryGraphRuntimeAsset _geometryGraphRuntimeData;

        public static void SetDebugOverwrite(ScriptableObject graphModelAssetModel, GeometryGraphRuntimeAsset data)
        {
            _debugOverwrite = (graphModelAssetModel, data);
        }

        public static void ClearDebugOverwrite()
        {
            _debugOverwrite = default;
        }

        public override NewGeometryGraphData GeometryGraphData
        {
            get
            {
                if (_geometryGraphRuntimeData != null)
                    return _geometryGraphRuntimeData.GeometryGraphData;
                return null;
            }
        }
    }
}