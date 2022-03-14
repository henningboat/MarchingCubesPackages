using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    namespace henningboat.CubeMarching.GeometryComponents
    {
        [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
        public struct CShapeBoundingBox : IGeometryShape
        {
            #region Static Stuff

            //SDF code from
            //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
            public static PackedFloat ComputeBoundingBoxDistance(in PackedFloat3 positionOS, in PackedFloat3 b, in PackedFloat e)
            {
                var p = abs(positionOS) - b;
                var q = abs(p + e) - e;
                return min(min(
                        length(max(PackedFloat3(p.x, q.y, q.z), 0.0f)) + min(max(p.x, max(q.y, q.z)), 0.0f),
                        length(max(PackedFloat3(q.x, p.y, q.z), 0.0f)) + min(max(q.x, max(p.y, q.z)), 0.0f)),
                    length(max(PackedFloat3(q.x, q.y, p.z), 0.0f)) + min(max(q.x, max(q.y, p.z)), 0.0f));
            }

            #endregion

            #region Public Fields

            [FieldOffset(0)] [DefaultValue(2)] public float boundWidth;

            [FieldOffset(4)] [DefaultValue(8, 8, 8)]
            public float3 extends;

            #endregion

            #region ITerrainModifierShape Members

            public PackedFloat GetSurfaceDistance(in PackedFloat3 positionOS, in BinaryDataStorage assetData,
                in GeometryInstruction instruction)
            {
                return ComputeBoundingBoxDistance(positionOS, extends, boundWidth);
            }

            public ShapeType Type => ShapeType.BoundingBox;

            #endregion
        }
    }
}