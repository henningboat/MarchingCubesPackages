using System;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    [Serializable]
    public struct GeometryInstruction
    {
        #region Public Fields

        public int CombinerDepth;
        public GeometryInstructionType GeometryInstructionType;

        public int GeometryInstructionSubType;

        public int32 PropertyIndexes;
        public float32 ResolvedPropertyValues;

        public GeometryInstructionFlags _flags;
        public bool HasMaterial => (_flags & GeometryInstructionFlags.HasMaterial) != 0;
        public bool WritesToDistanceField => GeometryInstructionType != GeometryInstructionType.PositionModification;
        public CombinerOperation CombinerBlendOperation;
        public float CombinerBlendFactor => ResolvedPropertyValues[15];

        public Hash128 GeometryInstructionHash;

        [FormerlySerializedAs("SourceLayerID")] [FormerlySerializedAs("SourceLayer")] public SerializableGUID ReferenceGUID;

        #endregion

        public void AddValueBufferOffset(int valueBufferOffset)
        {
            PropertyIndexes.AddOffset(valueBufferOffset);
        }

        public CGenericTerrainTransformation GetTerrainTransformation()
        {
            return new CGenericTerrainTransformation
            {
                Data = ResolvedPropertyValues,
                PositionModificationType = (PositionModificationType) GeometryInstructionSubType
            };
        }

        public CGenericGeometryShape GetShapeInstruction()
        {
            return new CGenericGeometryShape
                {Data = ResolvedPropertyValues, ShapeType = (ShapeType) GeometryInstructionSubType};
        }

        public CGenericDistanceModification GetDistanceModificationInstruction()
        {
            return new CGenericDistanceModification
                {Data = ResolvedPropertyValues, Type = (DistanceModificationType) GeometryInstructionSubType};
        }

        public unsafe float4x4 GetTransformation()
        {
            var resolvedPropertyValues = ResolvedPropertyValues;
            return UnsafeUtility.ReadArrayElement<float4x4>(UnsafeUtility.AddressOf(ref resolvedPropertyValues), 1);
        }

        public TerrainMaterial GetMaterialData()
        {
            var resolvedPropertyValue = ResolvedPropertyValues[14];
            return UnsafeCastHelper.Cast<float, TerrainMaterial>(ref resolvedPropertyValue);
        }

        /// <summary>
        /// Returns a hash of all data of the instruction that is actually used
        /// </summary>
        /// <returns></returns>
        public void UpdateHash()
        {
            Hash128 hash = default;
            hash.Append(ref GeometryInstructionType);
            hash.Append(ref GeometryInstructionSubType);
            hash.Append(ref CombinerBlendOperation);

            hash.Append(CombinerDepth);

            var ignoreTransformation = GeometryInstructionType == GeometryInstructionType.Combiner ||
                                       GeometryInstructionType == GeometryInstructionType.DistanceModification;

            if (ignoreTransformation)
            {
                var propertyData = UnsafeCastHelper.Cast<float32, float4x4>(ref ResolvedPropertyValues);
                hash.Append(ref propertyData);
            }
            else
            {
                hash.Append(ref ResolvedPropertyValues);
            }

            GeometryInstructionHash = hash;
        }
    }

    public unsafe struct AssetDataStorage:IDisposable
    {
        private NativeList<byte> DataBuffer;

        public AssetDataStorage(Allocator allocator)
        {
            DataBuffer = new NativeList<byte>(allocator);
        }

        public unsafe SDFData GetData()
        {
            SDFData result = new SDFData();
            var unsafeReadOnlyPtr = DataBuffer.GetUnsafeReadOnlyPtr();
            result.Size = UnsafeUtility.ReadArrayElement<int2>(unsafeReadOnlyPtr,0);
            result.Data = (float*) ((byte*)unsafeReadOnlyPtr + (ulong) sizeof(int2));
            return result;
        }

        public void AddSDFShape(SDFData sdf)
        {
            sdf.GetData(out void* headerPointer, out int headerSize, out void* dataPointer, out int dataSize);
            DataBuffer.Capacity += headerSize + dataSize;
            DataBuffer.Length += headerSize + dataSize;

            var dataBufferPointer = DataBuffer.GetUnsafePtr();
            UnsafeUtility.MemCpy(dataBufferPointer,headerPointer,headerSize);
            UnsafeUtility.MemCpy((byte*)dataBufferPointer + (ulong) headerSize, dataPointer, dataSize);
        }

        public void Dispose()
        {
            DataBuffer.Dispose();
        }
    }

    public unsafe struct SDFData: IDisposable
    {
        public int2 Size;
        public float* Data;

        public int DataSize => Size.x * Size.y * sizeof(float);
        
        public SDFData(Texture2D sdfTexture)
        {
            Size = new int2(sdfTexture.width, sdfTexture.height);
            Data = (float*)UnsafeUtility.Malloc(Size.x * Size.y * sizeof(float), 4, Allocator.Temp);

            var textureData = sdfTexture.GetPixels();
            for (int i = 0; i < textureData.Length; i++)
            {
                Data[i] = textureData[i].a;
            }
        }

        public void GetData(out void* headerPointer, out int headerSize, out void* dataPointer, out int dataSize)
        {
            headerPointer = UnsafeUtility.AddressOf(ref Size);
            headerSize = sizeof(int2);
            dataPointer = Data;
            dataSize = DataSize;
        }

        public float Sample(float2 uv)
        {
            uv = clamp(uv, 0, Size - 2);
            int2 flooredUV = (int2) floor(uv);

            float c00 = SamplePixel(flooredUV);
            float c10 = SamplePixel(flooredUV + int2(1, 0));
            float c01 = SamplePixel(flooredUV + int2(0, 1));
            float c11 = SamplePixel(flooredUV + int2(1, 1));

            float2 subPixelPosition = uv % 1;
            float row0 = lerp(c00, c10, subPixelPosition.x);
            float row1 = lerp(c01, c11, subPixelPosition.x);

            float interpolatedValue = lerp(row0, row1, subPixelPosition.y);
            return interpolatedValue;
        }
      

        public float SamplePixel(int2 pixel)
        {
            int index = Utils.PositionToIndex(pixel, Size);
            return Data[index];
        }
        
        public void Dispose()
        {
            UnsafeUtility.Free(Data, Allocator.Temp);
        }
    }
}