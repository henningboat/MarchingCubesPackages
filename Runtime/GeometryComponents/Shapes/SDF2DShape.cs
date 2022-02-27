using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct SDF2DShape : IGeometryShape, IConvertGUIDReferenceToDataBufferOffset
    {
        [FieldOffset(0)] [DefaultValue(1f)] public float scale;
        [FieldOffset(4)] [DefaultValue(0.5f)] public float offset;
        [FieldOffset(8)] [DefaultValue(10)] public float distanceScale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, AssetDataStorage assetData,
            GeometryInstruction instruction)
        {
            positionOS *= scale;
            unsafe
            {
                PackedFloat result = default;
                SDF2DData* shape = assetData.GetData<SDF2DData>(instruction.assetIndexInGlobalBuffer);
                for (int i = 0; i < 4; i++)
                {
                    float2 uv = new float2(positionOS.x.PackedValues[i], positionOS.y.PackedValues[i]);
                    result.PackedValues[i] = shape->Sample(uv) * distanceScale;
                }

                return result;
            }
        }

        public ShapeType Type => ShapeType.SDF2D;
    }
}