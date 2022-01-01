using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    public class DistanceValueProvider : GeometryPropertyValueProvider
    {
        public override float[] GetValue(GeometryGraphInstance graphInstance, GeometryPropertyType geometryPropertyType)
        {
            switch (geometryPropertyType)
            {
                case GeometryPropertyType.Float:
                    return new[] {math.distance(graphInstance.transform.position, transform.position)};
                case GeometryPropertyType.Float3:
                    var result = math.abs(graphInstance.transform.position - transform.position);
                    return new[] {result.x, result.y, result.z};
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometryPropertyType), geometryPropertyType, null);
            }
        }
    }
}