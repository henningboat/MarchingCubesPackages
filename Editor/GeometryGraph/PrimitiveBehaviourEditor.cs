using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.Utils.Containers;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(NewShapeProxy))]
    public class PrimitiveBehaviourEditor : UnityEditor.Editor
    {
        private SerializedProperty _shapeType;

        private List<string> _properties = new();

        private void OnEnable()
        {
            _shapeType = serializedObject.FindProperty("_shapeType");
            UpdateProperties((ShapeType) _shapeType.enumValueIndex, (NewShapeProxy) target);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_shapeType);

            foreach (var property in _properties) GUILayout.Label(property);

            serializedObject.ApplyModifiedProperties();

            var instance = target as NewShapeProxy;
            var data = instance.GeometryGraphData;

            GUILayout.Space(10);

            var geometryGraphInstance = (GeometryInstanceBase) target;
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

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                geometryGraphInstance.SetOverwrites(currentOverwrites);

                EditorUtility.SetDirty(geometryGraphInstance);

                UpdateProperties((ShapeType) _shapeType.enumValueIndex, instance);
            }
        }

        private void ResetProperty(GeometryGraphProperty variable, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            currentOverwrite.Reset(variable);
        }

        private void UpdateProperties(ShapeType shapeType, NewShapeProxy target)
        {
            using (var context = new RuntimeGeometryGraphResolverContext())
            {
                var exposedProperties = new List<GeometryGraphProperty>();
                foreach (var property in GeometryTypeCache.GetPropertiesForType(shapeType))
                {
                    float32 defaultValue = default;
                    if (property.Item3.Length != 0) defaultValue = float32.GetFloat32FromFloatArray(property.Item3);

                    exposedProperties.Add(context.CreateOrGetExposedProperty(new SerializableGUID(property.Item2),
                        property.Item2, property.Item1, defaultValue));
                }

                var shapeProxy =
                    new GenericShapeProxy(shapeType, context.OriginTransformation, exposedProperties.ToArray());

                context.AddShape(shapeProxy);
                target.Initialize(context.GetGeometryGraphData());
            }
        }
    }
}