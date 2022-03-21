using System.Collections.Generic;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    [CustomEditor(typeof(PrimitiveInstance))]
    public class GeometryPrimitiveInstanceEditor : GeometryInstanceEditor
    {
        private SerializedProperty _shapeType;

        protected override void OnEnable()
        {
            base.OnEnable();
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
            //if (GUI.changed)
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


                var hasColorDecorator = TryGetDecorator(out ColorDecorator colorDecorator);
                var hasInflation = TryGetDecorator(out InflationDecorator inflation);
                var hasRepetition = TryGetDecorator(out RepetitionDecorator repetition);
                var hasOnion = TryGetDecorator(out OnionDecorator onion);
                var hasInversion = TryGetDecorator(out InversionDecorator _);
                var hasTwist = TryGetDecorator(out TwistDecorator twist);

                var color = Color.white;
                if (hasColorDecorator)
                {
                    color = colorDecorator.Color;
                }

                GeometryGraphProperty colorFloat3Property =
                    context.CreateProperty(new Vector3(color.r, color.g, color.b));
                context.AddMathInstruction(MathOperatorType.Float3ToColor32, GeometryPropertyType.Color32,
                    colorFloat3Property, context.ZeroFloatProperty, out GeometryGraphProperty colorProperty);

                context.PushColor(colorProperty);


                if (hasRepetition)
                {
                    context.PushCombiner(CombinerOperation.Min, context.ZeroFloatProperty);
                    float3 offset = repetition.Size * 0.5f;
                   //  context.PushTransformation(context.CreateProperty(Matrix4x4.Translate(offset)), true);
                    context.WriteInstruction(GeometryInstructionUtility.CreateInstruction(
                        GeometryInstructionType.PositionModification, (int) PositionModificationType.Repetition,
                        new List<GeometryGraphProperty>()
                        {
                            context.CreateProperty(repetition.Size),
                        }));
                    context.PushTransformation(context.ZeroTransformation, false);
                }

                if (hasTwist)
                {   
                    context.PushCombiner(CombinerOperation.Min, context.ZeroFloatProperty);
                    context.WriteInstruction(GeometryInstructionUtility.CreateInstruction(
                    GeometryInstructionType.PositionModification, (int) PositionModificationType.Twist,
                    new List<GeometryGraphProperty>()
                    {
                        context.CreateProperty(twist.Twist),
                    }));
                    
                    context.PushTransformation(context.ZeroTransformation, false);
                }

                var shapeInstruction = GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape,
                    _shapeType.enumValueIndex, exposedProperties);
                context.WriteInstruction(shapeInstruction);


                if (hasInflation)
                {
                    context.WriteInstruction(GeometryInstructionUtility.CreateInstruction(
                        GeometryInstructionType.DistanceModification, (int) DistanceModificationType.Inflation,
                        new List<GeometryGraphProperty>() {context.CreateProperty(inflation.Inflation)}));
                }
                if (hasOnion)
                {
                    context.WriteInstruction(GeometryInstructionUtility.CreateInstruction(
                        GeometryInstructionType.DistanceModification, (int) DistanceModificationType.Onion,
                        new List<GeometryGraphProperty>() {context.CreateProperty(onion.Thickness)}));
                }

                if (hasInversion)
                {
                    context.WriteInstruction(GeometryInstructionUtility.CreateInstruction(
                        GeometryInstructionType.DistanceModification, (int) DistanceModificationType.Inversion, new List<GeometryGraphProperty>()));
                }

                if (hasTwist)
                {
                    context.PopTransformation();
                    context.PopCombiner();
                }
                
                if (hasRepetition)
                {
                    context.PopTransformation();
                    context.PopCombiner();
                }

                
                context.PopColor();

                target.Initialize(context.GetGeometryGraphData());
            }
        }

        private bool TryGetDecorator<T>(out T decorator) where T : Component
        {
            decorator = ((MonoBehaviour) target).gameObject.GetComponent<T>();
            return decorator != null;
        }
    }
}