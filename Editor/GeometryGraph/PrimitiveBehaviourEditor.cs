using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            foreach (var property in _properties)
            {
                GUILayout.Label(property);
            }

            serializedObject.ApplyModifiedProperties();

            var instance = target as NewShapeProxy;
            var data = instance.GeometryGraphData;
           
              GUILayout.Space(10);
            
            var geometryGraphInstance = (GeometryInstanceBase) target;
            var currentOverwrites = geometryGraphInstance.GetOverwrites();
            
            foreach (var variableDeclarationModel in data.Variables)
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
                    currentOverwrite.Value = new float32()
                    {
                        [0] = EditorGUILayout.FloatField(variableDeclarationModel.Name, currentOverwrite.Value[0])
                    };
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
                
                UpdateProperties((ShapeType) _shapeType.enumValueIndex,instance);
                
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
                List<GeometryGraphProperty> exposedProperties = new List<GeometryGraphProperty>();
                foreach (var property in GeometryTypeCache.GetPropertiesForType(shapeType))
                {
                    exposedProperties.Add(context.CreateOrGetExposedProperty(new SerializableGUID(property.Item2),
                        property.Item2, property.Item1));
                }

                var shapeProxy = new GenericShapeProxy(shapeType, context.OriginTransformation, exposedProperties.ToArray());

                context.AddShape(shapeProxy);
                target.Initialize(context.GetGeometryGraphData());
            }
        }
    }
    
    public static class GeometryTypeCache
    {
        private static readonly Dictionary<ShapeType, Type> ShapeTypes;
        private static readonly Dictionary<ShapeType, List<(GeometryPropertyType,string)>> PropertiesOfShapeType;

        static GeometryTypeCache()
        {
            try
            {
                ShapeTypes = new Dictionary<ShapeType, Type>();
                PropertiesOfShapeType = new();
                
                foreach (var type in TypeCache.GetTypesWithAttribute<ShapeProxyAttribute>().ToArray())
                {
                    var attribute =
                        type.GetCustomAttribute(typeof(ShapeProxyAttribute)) as ShapeProxyAttribute;
                    ShapeTypes.Add(attribute.ShapeType, type);

                    //var typeInstance = Activator.CreateInstance(type) as ShapeProxy;
                    PropertiesOfShapeType.Add(attribute.ShapeType,type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => info.FieldType.IsAssignableFrom(typeof(GeometryGraphProperty))).Select(info =>
                    {
                        return (GeometryPropertyType.Float,info.Name);
                    }).ToList());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Geometry Type Cache failed to initialize. This will break everything :/ The error was: \n + {e}");
                throw;
            }
        }

        public static List<(GeometryPropertyType,string)> GetPropertiesForType(ShapeType shapeType)
        {
            return PropertiesOfShapeType[shapeType];
        }
    }
}