using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.SIMDMath;
using henningboat.CubeMarching.PrimitiveBehaviours;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{[ShapeProxy(ShapeType.BoundingBox)]
    public class BoundingBoxShapeProxy : ShapeProxy
    {
        private GeometryGraphProperty _extends;
        private GeometryGraphProperty _boundsWidth;

        public BoundingBoxShapeProxy(GeometryGraphProperty extends, GeometryGraphProperty boundsWidth,
            GeometryGraphProperty transformation) : base(transformation)
        {
            _extends = extends;
            _boundsWidth = boundsWidth;
        }

        protected override List<GeometryGraphProperty> GetProperties()
        {
            throw new NotImplementedException();
        }

        public override ShapeType ShapeType => ShapeType.BoundingBox;
    }
    
    
    namespace henningboat.CubeMarching.GeometryComponents
    {
        [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
        public struct CShapeBoundingBox: IGeometryShapeResolver
        {

            #region Static Stuff

            //SDF code from
            //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
            public static PackedFloat ComputeBoundingBoxDistance(PackedFloat3 p, PackedFloat3 b, PackedFloat e)
            {
                p = abs(p) - b;
                var q = abs(p + e) - e;
                return min(min(
                        length(max(PackedFloat3(p.x, q.y, q.z), 0.0f)) + min(max(p.x, max(q.y, q.z)), 0.0f),
                        length(max(PackedFloat3(q.x, p.y, q.z), 0.0f)) + min(max(q.x, max(p.y, q.z)), 0.0f)),
                    length(max(PackedFloat3(q.x, q.y, p.z), 0.0f)) + min(max(q.x, max(q.y, p.z)), 0.0f));
            }

            #endregion

            #region Public Fields

            [FieldOffset(0)] public float boundWidth;
            [FieldOffset(4)] public float3 extends;

            #endregion

            #region ITerrainModifierShape Members

            public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
            {
                return ComputeBoundingBoxDistance(positionOS, extends, boundWidth);
            }

            #endregion

        }
    }
}