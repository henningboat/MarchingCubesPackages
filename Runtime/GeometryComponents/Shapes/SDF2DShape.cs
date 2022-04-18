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
        [FieldOffset(16)] [DefaultValue(50f)] public float thickness;


        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            var binaryDataStorage = assetData;
            binaryDataStorage.GetBinaryAsset(sdf.AssetIndex, out SDF2DHeader header,
                out NativeSlice<float> sdfData);
            
            for (int i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                var surfaceDistance = GetSurfaceDistance(positionOS, header, sdfData);
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
        }

        private PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, SDF2DHeader header, NativeSlice<float> sdfData)
        {
            PackedFloat result = default;

            var scaledPosition = (positionOS * scale + new PackedFloat3(new float3((float2) header.Size * 0.5f, 1)));

            for (int i = 0; i < 4; i++)
            {
                float2 uv = new float2(scaledPosition.x.PackedValues[i], scaledPosition.y.PackedValues[i]);
                result.PackedValues[i] = (header.Sample(uv, sdfData)) * distanceScale - offset;
            }

            result = SimdMath.max(SimdMath.abs(positionOS.z) - thickness, result);

            return result / scale;
        }

        public ShapeType Type => ShapeType.SDF2D;
    }


}