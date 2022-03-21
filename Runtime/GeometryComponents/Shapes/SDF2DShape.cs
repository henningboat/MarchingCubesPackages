using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Collections;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct SDF2DShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(1f)] public float scale;
        [FieldOffset(4)] [DefaultValue(0f)] public float offset;
        [FieldOffset(8)] [DefaultValue(10)] public float distanceScale;
        [FieldOffset(12)] public SDF2DAssetReference sdf;

        public PackedFloat GetSurfaceDistance(in PackedFloat3 positionOS, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            PackedFloat result = default;
            //todo figure out how to get the binary asset without copying the struct
            var binaryDataStorage = assetData;
            binaryDataStorage.GetBinaryAsset(sdf.AssetIndex, out SDF2DHeader header,
                out NativeSlice<float> sdfData);

            var scaledPosition = (positionOS + (new PackedFloat3(new float3((float2) header.Size, 1f)) * scale));

            for (int i = 0; i < 4; i++)
            {
                float2 uv = new float2(scaledPosition.x.PackedValues[i], scaledPosition.y.PackedValues[i]);
                result.PackedValues[i] = (header.Sample(uv, sdfData)) * distanceScale - offset;
            }

            return result;
        }

        public ShapeType Type => ShapeType.SDF2D;
    }


}