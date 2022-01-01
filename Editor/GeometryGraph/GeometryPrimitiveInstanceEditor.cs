using System.Collections.Generic;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(PrimitiveInstance))]
    public class GeometryPrimitiveInstanceEditor : GeometryInstanceEditor
    {
        private SerializedProperty _shapeType;

        private void OnEnable()
        {
            _shapeType = serializedObject.FindProperty("_shapeType");
            UpdateShapeInstructions((ShapeType) _shapeType.enumValueIndex, (PrimitiveInstance) target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.PropertyField(_shapeType);

            var instance = target as GeometryInstance;
            var data = instance.GeometryInstructionList;

            GUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                Undo.RecordObject(target, "changed overwrite");

                UpdateShapeInstructions((ShapeType) _shapeType.enumValueIndex, (PrimitiveInstance) instance);
            }

            base.OnInspectorGUI();
        }

        private void ResetProperty(GeometryGraphProperty variable, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            currentOverwrite.Reset(variable);
        }

        private void UpdateShapeInstructions(ShapeType shapeType, PrimitiveInstance target)
        {
            using (var context = new GeometryInstructionListBuilder())
            {
                var exposedProperties = new List<GeometryGraphProperty>();
                foreach (var property in GeometryTypeCache.GetPropertiesForType(shapeType))
                {
                    float32 defaultValue = default;
                    if (property.Item3.Length != 0) defaultValue = float32.GetFloat32FromFloatArray(property.Item3);

                    exposedProperties.Add(context.CreateOrGetExposedProperty(new SerializableGUID(property.Item2),
                        property.Item2, property.Item1, defaultValue));
                }

                var shapeInstruction = GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape,
                    _shapeType.enumValueIndex, exposedProperties);
                context.WriteInstruction(shapeInstruction);
                target.Initialize(context.GetGeometryGraphData());
            }
        }
    }
}