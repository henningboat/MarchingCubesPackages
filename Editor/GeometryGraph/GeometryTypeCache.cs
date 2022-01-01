using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using UnityEditor;
using UnityEngine;

namespace Editor.GeometryGraph
{
    internal class GeometryTypeCacheCollection<TInterface>
    {
        private readonly Dictionary<int, Type> ShapeTypes = new();
        private readonly Dictionary<int, List<(GeometryPropertyType, string, float[])>> PropertiesOfShapeType = new();

        public GeometryTypeCacheCollection()
        {
            try
            {
                var getTypePropertyInfo = typeof(TInterface).GetProperty("Type");


                ShapeTypes = new Dictionary<int, Type>();
                PropertiesOfShapeType = new Dictionary<int, List<(GeometryPropertyType, string, float[])>>();

                foreach (var type in TypeCache.GetTypesDerivedFrom<TInterface>().ToArray())
                {
                    var instance = (TInterface) Activator.CreateInstance(type);
                    var value = getTypePropertyInfo.GetValue(instance);
                    var enumTypeValue = Convert.ToInt32(value);
                    ShapeTypes.Add(enumTypeValue, type);

                    //var typeInstance = Activator.CreateInstance(type) as ShapeProxy;
                    PropertiesOfShapeType.Add(enumTypeValue, type
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

        public List<(GeometryPropertyType, string, float[])> GetPropertiesOfType(int enumTypeValue)
        {
            return PropertiesOfShapeType[enumTypeValue];
        }
    }

    public static class GeometryTypeCache
    {
        private static GeometryTypeCacheCollection<IGeometryShape> ShapeTypeInfo = new();
        private static GeometryTypeCacheCollection<IPositionModification> PositionModificationInfo = new();
        private static GeometryTypeCacheCollection<IDistanceModification> DistanceModificationInfo = new();

        public static List<(GeometryPropertyType, string, float[])> GetPropertiesForType(Enum enumValue)
        {
            switch (enumValue)
            {
                case ShapeType shapeType:
                    return ShapeTypeInfo.GetPropertiesOfType((int) shapeType);
                case PositionModificationType transformationType:
                    return PositionModificationInfo.GetPropertiesOfType((int) transformationType);
                case DistanceModificationType distanceModificationType:
                    return DistanceModificationInfo.GetPropertiesOfType((int) distanceModificationType);
            }

            throw new ArgumentException();
        }
    }
}