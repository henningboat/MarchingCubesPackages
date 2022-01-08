using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using UnityEditor;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(GeometryGraphInstance))]
    public class GeometryGraphInstanceEditor : GeometryInstanceEditor
    {
        private SerializedProperty _geometryGraphRuntimeData;

        protected override void OnEnable()
        {
            base.OnEnable();
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