using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using UnityEditor;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(GeometryGraphInstance))]
    public class GeometryGraphInstanceEditor : GeometryInstanceEditor
    {
        private SerializedProperty _geometryGraphRuntimeData;

        private void OnEnable()
        {
            _geometryGraphRuntimeData = serializedObject.FindProperty("_geometryGraphRuntimeData");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_geometryGraphRuntimeData);
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}