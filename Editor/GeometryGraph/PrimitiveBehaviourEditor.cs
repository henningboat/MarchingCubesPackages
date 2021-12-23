using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor;
using UnityEngine;

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
            UpdateProperties();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_shapeType);
         
            if (GUI.changed)
            {
                UpdateProperties();
            }

            foreach (var property in _properties)
            {
                GUILayout.Label(property);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateProperties()
        {
            _properties = GeometryTypeCache.GetPropertiesForType((ShapeType) _shapeType.enumValueIndex);
        }
    }

    public static class GeometryTypeCache
    {
        private static readonly Dictionary<ShapeType, Type> ShapeTypes;
        private static readonly Dictionary<ShapeType, List<string>> PropertiesOfShapeType;

        static GeometryTypeCache()
        {
            try
            {
                ShapeTypes = new Dictionary<ShapeType, Type>();
                PropertiesOfShapeType = new Dictionary<ShapeType, List<string>>();
                
                foreach (var type in TypeCache.GetTypesWithAttribute<ShapeProxyAttribute>().ToArray())
                {
                    var attribute =
                        type.GetCustomAttribute(typeof(ShapeProxyAttribute)) as ShapeProxyAttribute;
                    ShapeTypes.Add(attribute.ShapeType, type);

                    //var typeInstance = Activator.CreateInstance(type) as ShapeProxy;
                    PropertiesOfShapeType.Add(attribute.ShapeType,type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => info.FieldType.IsAssignableFrom(typeof(GeometryGraphProperty))).Select(info => info.Name).ToList());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Geometry Type Cache failed to initialize. This will break everything :/ The error was: \n + {e}");
                throw;
            }
        }

        public static List<string> GetPropertiesForType(ShapeType shapeType)
        {
            return PropertiesOfShapeType[shapeType];
        }
    }
}