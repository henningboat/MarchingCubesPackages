using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.PrimitiveBehaviours;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    [ShapeProxy(ShapeType.Voronoi)]
    public class VoronoiShapeProxy : ShapeProxy
    {
        [PropertyType(GeometryPropertyType.Float)]
        private GeometryGraphProperty _valueOffset;

        [PropertyType(GeometryPropertyType.Float3)]
        private GeometryGraphProperty _scale;

        public VoronoiShapeProxy(GeometryGraphProperty valueOffset, GeometryGraphProperty scale,
            GeometryGraphProperty transformation) : base(transformation)
        {
            _valueOffset = valueOffset;
            _scale = scale;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>()
            {
                _valueOffset,
                _scale
            };
        }

        public override ShapeType ShapeType => ShapeType.Voronoi;
    }


    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct VoronoiShapeResolver : IGeometryShapeResolver
    {
        [FieldOffset(0)] public float valueOffset;
        [FieldOffset(4)] public float3 scale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            positionOS *= scale;
            return (Voronoi(positionOS) - new PackedFloat(valueOffset)) / scale.x;
        }


        public PackedFloat3 Hash(PackedFloat3 x)
        {
            x = new PackedFloat3(
                dot(x, new PackedFloat3(new PackedFloat(127.1f), new PackedFloat(311.7f), new PackedFloat(74.7f))),
                dot(x, new PackedFloat3(new PackedFloat(269.5f), new PackedFloat(183.3f), new PackedFloat(246.1f))),
                dot(x, new PackedFloat3(new PackedFloat(113.5f), new PackedFloat(271.9f), new PackedFloat(124.6f))));

            return frac(sin(x) * 43758.5453123f);
        }

        //from https://www.shadertoy.com/view/ldl3Dl
        public PackedFloat Voronoi(PackedFloat3 x)
        {
            var p = floor(x);
            var f = frac(x);

            PackedFloat id = 0.0f;

            var res = new PackedFloat(100f);
            var res2 = new PackedFloat(100f);


            for (var k = -1; k <= 1; k++)
            for (var j = -1; j <= 1; j++)
            for (var i = -1; i <= 1; i++)
            {
                var b = new PackedFloat3(new PackedFloat(i), new PackedFloat(j), new PackedFloat(k));
                var r = b - f + Hash(p + b);
                var d = dot(r, r);

                var newDistanceIsSmallest = d.PackedValues < res.PackedValues;

                res.PackedValues = math.@select(res.PackedValues, d.PackedValues, newDistanceIsSmallest);
                res2.PackedValues = math.@select(min(res2, d).PackedValues, res2.PackedValues,
                    newDistanceIsSmallest);
            }

            return -(res2 - res);
        }
    }
}