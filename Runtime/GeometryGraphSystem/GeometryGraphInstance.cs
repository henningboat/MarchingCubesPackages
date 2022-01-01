using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryGraphSystem
{
    [ExecuteInEditMode]
    public class GeometryGraphInstance : GeometryInstance
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

        public override GeometryInstructionList GeometryInstructionList
        {
            get
            {
                if (_geometryGraphRuntimeData != null)
                    return _geometryGraphRuntimeData.GeometryInstructionList;
                return null;
            }
        }
    }
}