using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Editor.GeometryGraph
{
    public static class GeometryTypeCache
    {
        private static readonly Dictionary<ShapeType, Type> ShapeTypes;

        private static readonly Dictionary<ShapeType, List<(GeometryPropertyType, string, float[])>>
            PropertiesOfShapeType;

        static GeometryTypeCache()
        {
            try
            {
                ShapeTypes = new Dictionary<ShapeType, Type>();
                PropertiesOfShapeType = new Dictionary<ShapeType, List<(GeometryPropertyType, string, float[])>>();

                foreach (var type in TypeCache.GetTypesDerivedFrom<IGeometryShape>().ToArray())
                {
                    var instance = (IGeometryShape) Activator.CreateInstance(type);
                    ShapeTypes.Add(instance.ShapeType, type);

                    //var typeInstance = Activator.CreateInstance(type) as ShapeProxy;
                    PropertiesOfShapeType.Add(instance.ShapeType, type
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(info =>
                        {
                            var defaultValue = new float[0];
                            var defaultValueAttribute = info.GetCustomAttribute<DefaultValueAttribute>();
                            if (defaultValueAttribute != null) defaultValue = defaultValueAttribute.DefaultValue;
                            return (info.FieldType.GetGeometryPropertyTypeFromType(), info.Name, defaultValue);
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

        public static List<(GeometryPropertyType, string, float[])> GetPropertiesForType(ShapeType shapeType)
        {
            return PropertiesOfShapeType[shapeType];
        }
    }
}