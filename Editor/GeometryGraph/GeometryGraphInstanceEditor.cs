using System;
using System.Linq;
using henningboat.CubeMarching;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Object = UnityEngine.Object;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    [CustomEditor(typeof(GeometryGraphInstance))]
    public class GeometryGraphInstanceEditor : UnityEditor.Editor
    {
        private SerializedProperty _geometryGraphProperty;
        private SerializedProperty _overwritesProperty;

        private void OnEnable()
        {
            _geometryGraphProperty = serializedObject.FindProperty("_geometryGraphRuntimeData");
        }

        public override void OnInspectorGUI()
        {
            var graph = _geometryGraphProperty.objectReferenceValue as GeometryGraphRuntimeData;
            var graphAsset = AssetDatabase.LoadAssetAtPath<GeometryGraphAsset>(AssetDatabase.GetAssetPath(graph));

            EditorGUILayout.PropertyField(_geometryGraphProperty);
            if (graph == null)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField(graph.name);

            GUILayout.Space(10);

            var geometryGraphInstance = (GeometryGraphInstance) target;
            var currentOverwrites = geometryGraphInstance.GetOverwrites();

            foreach (var variableDeclarationModel in graph.Variables)
            {
                EditorGUILayout.BeginHorizontal();

                var currentOverwrite = currentOverwrites.FirstOrDefault(overwrite => overwrite.PropertyGUID == variableDeclarationModel.ID);
                if (currentOverwrite == null)
                {
                    currentOverwrite = new GeometryGraphPropertyOverwrite(variableDeclarationModel.ID);
                    currentOverwrites.Add(currentOverwrite);

                    ResetProperty(variableDeclarationModel, currentOverwrite);
                }

                if (variableDeclarationModel.Type == GeometryPropertyType.Float)
                {
                    currentOverwrite.SetValueCapacity(1);
                    currentOverwrite.Value[0] = EditorGUILayout.FloatField(variableDeclarationModel.Name, currentOverwrite.Value[0]);
                }

                if (variableDeclarationModel.Type==GeometryPropertyType.Float3)
                {
                    currentOverwrite.SetValueCapacity(3);

                    var currentValue = new Vector3(currentOverwrite.Value[0], currentOverwrite.Value[1], currentOverwrite.Value[2]);
                    currentValue = EditorGUILayout.Vector3Field(variableDeclarationModel.Name, currentValue);

                    currentOverwrite.Value[0] = currentValue.x;
                    currentOverwrite.Value[1] = currentValue.y;
                    currentOverwrite.Value[2] = currentValue.z;
                }

                if (GUILayout.Button("Reset"))
                {
                    ResetProperty(variableDeclarationModel, currentOverwrite);
                }

                var valueProvider = EditorGUILayout.ObjectField("", currentOverwrite.ProviderObject, typeof(GeometryPropertyValueProvider), true) as GeometryPropertyValueProvider;
                currentOverwrite.SetProviderObject(valueProvider);

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                geometryGraphInstance.SetOverwrites(currentOverwrites);
                EditorUtility.SetDirty(geometryGraphInstance);
            }
        }

        private void ResetProperty(ExposedVariable variable, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            currentOverwrite.Reset(variable);
        }
    }
}