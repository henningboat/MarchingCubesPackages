using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [ShapeProxy(ShapeType.Box)]
    public class BoxShapeProxy : ShapeProxy
    {
        [PropertyTypeAttribute(GeometryPropertyType.Float3)] private GeometryGraphProperty _extends;

        public BoxShapeProxy(GeometryGraphProperty extends, GeometryGraphProperty transformation) : base(transformation)
        {
            _extends = extends;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            return new List<GeometryGraphProperty>()
            {
                _extends
            };
        }

        public override ShapeType ShapeType => ShapeType.Box;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct BoxShapeResolver : IGeometryShapeResolver
    {
        [FieldOffset(0)] public float3 Extends;
        
        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            var extends = Extends;
            PackedFloat3 q = abs(positionWS) - extends;
            return length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
        }

        public ShapeType Type => ShapeType.Box;

    }
}