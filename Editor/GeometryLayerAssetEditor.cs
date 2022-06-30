using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GeometryLayerAsset))]
    public class GeometryLayerAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}