using System.Linq;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.Utils.Containers;
using UnityEditor;
using UnityEngine;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(GeometryInstance),true)]
    public class GeometryInstanceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as GeometryInstance;
            var data = instance.GeometryInstructionList;

            GUILayout.Space(10);

            var geometryGraphInstance = (GeometryInstance) target;
            var currentOverwrites = geometryGraphInstance.GetOverwrites();

            foreach (var variableDeclarationModel in data.Variables)
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                var currentOverwrite =
                    currentOverwrites.FirstOrDefault(overwrite =>
                        overwrite.PropertyGUID == variableDeclarationModel.ID);
                if (currentOverwrite == null)
                {
                    currentOverwrite = new GeometryGraphPropertyOverwrite(variableDeclarationModel.ID);
                    currentOverwrites.Add(currentOverwrite);

                    ResetProperty(variableDeclarationModel, currentOverwrite);
                }

                if (variableDeclarationModel.Type == GeometryPropertyType.Float)
                    currentOverwrite.Value = new float32()
                    {
                        [0] = EditorGUILayout.FloatField(variableDeclarationModel.Name, currentOverwrite.Value[0])
                    };

                if (variableDeclarationModel.Type == GeometryPropertyType.Float3)
                {
                    var currentValue = new Vector3(currentOverwrite.Value[0], currentOverwrite.Value[1],
                        currentOverwrite.Value[2]);
                    currentValue = EditorGUILayout.Vector3Field(variableDeclarationModel.Name, currentValue);

                    currentOverwrite.Value = new float32()
                    {
                        [0] = currentValue.x,
                        [1] = currentValue.y,
                        [2] = currentValue.z
                    };
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Reset")) ResetProperty(variableDeclarationModel, currentOverwrite);

                var valueProvider =
                    EditorGUILayout.ObjectField("", currentOverwrite.ProviderObject,
                        typeof(GeometryPropertyValueProvider), true) as GeometryPropertyValueProvider;
                currentOverwrite.SetProviderObject(valueProvider);

                EditorGUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                Undo.RecordObject(target, "changed overwrite");
                geometryGraphInstance.SetOverwrites(currentOverwrites);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ResetProperty(GeometryGraphProperty variable, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            currentOverwrite.Reset(variable);
        }
    }
}