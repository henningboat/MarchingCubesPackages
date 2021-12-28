using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(NewShapeProxy))]
    public class GeometryPrimitiveInstanceEditor : GeometryInstanceEditor
    {
        private SerializedProperty _shapeType;

        private void OnEnable()
        {
            _shapeType = serializedObject.FindProperty("_shapeType");
            UpdateShapeInstructions((ShapeType) _shapeType.enumValueIndex, (NewShapeProxy) target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.PropertyField(_shapeType);

            var instance = target as GeometryInstanceBase;
            var data = instance.GeometryGraphData;

            GUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                Undo.RecordObject(target, "changed overwrite");

                UpdateShapeInstructions((ShapeType) _shapeType.enumValueIndex, (NewShapeProxy) instance);
            }

            base.OnInspectorGUI();
        }

        private void ResetProperty(GeometryGraphProperty variable, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            currentOverwrite.Reset(variable);
        }

        private void UpdateShapeInstructions(ShapeType shapeType, NewShapeProxy target)
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

                var shapeInstruction = GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape,
                    _shapeType.enumValueIndex, context.OriginTransformation, exposedProperties);
                context.WriteInstruction(shapeInstruction);
                target.Initialize(context.GetGeometryGraphData());
            }
        }
    }
}