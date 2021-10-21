using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeBoundingBox: ITerrainModifierShape
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

    [FieldOffset(0)] public FloatValue boundWidth;
    [FieldOffset(4)] public Float3Value extends;

    #endregion

    #region ITerrainModifierShape Members

    public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
    {
        return ComputeBoundingBoxDistance(positionOS, extends.Resolve(valueBuffer), boundWidth.Resolve(valueBuffer));
    }
    
    public ShapeType Type => ShapeType.BoundingBox;

    #endregion

    }
}