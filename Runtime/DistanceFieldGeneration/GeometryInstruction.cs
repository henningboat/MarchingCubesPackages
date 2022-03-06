using System;
using System.Runtime.InteropServices;
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
        ///     Returns a hash of all data of the instruction that is actually used
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

        [FormerlySerializedAs("SourceLayerID")] [FormerlySerializedAs("SourceLayer")]
        public SerializableGUID ReferenceGUID;

        //If the instruction requires to read from an asset (for example SDF), this will be set to the index of
        //that asset in the GeometryInstructionList. 
        public int assetReferenceIndex;
        
        public int assetIndexInGlobalBuffer;

        #endregion
    }


    /// <summary>
    ///     Important: Only pass this struct by reference, since the actual data is stored behind it inside
    ///     AssetDataStorage.DataBuffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SDF2DData : IDisposable, IBinaryAsset
    {
        public int2 Size;

        public float* Data => (float*) UnsafeUtility.AddressOf(ref Size) + sizeof(SDF2DData);

        public int DataSize => Size.x * Size.y * sizeof(float);

        public static void Create(Texture2D sdfTexture, AssetDataStorage dataStorage)
        {
            var size = new int2(sdfTexture.width, sdfTexture.height);
            var sdf2DData =
                dataStorage.CreateAsset<SDF2DData>(sdfTexture.GetInstanceID(), size.x * size.y * sizeof(float));

            var textureData = sdfTexture.GetPixels();
            sdf2DData->Size = size;

            var dataBuffer = UnsafeUtils.GetDataBuffer<float, SDF2DData>(sdf2DData);

            for (var i = 0; i < size.x * size.y; i++) dataBuffer[i] = (0.5f-textureData[i].r);
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
            var flooredUV = (int2) floor(uv);

            var c00 = SamplePixel(flooredUV);
            var c10 = SamplePixel(flooredUV + int2(1, 0));
            var c01 = SamplePixel(flooredUV + int2(0, 1));
            var c11 = SamplePixel(flooredUV + int2(1, 1));

            var subPixelPosition = uv % 1;
            var row0 = lerp(c00, c10, subPixelPosition.x);
            var row1 = lerp(c01, c11, subPixelPosition.x);

            var interpolatedValue = lerp(row0, row1, subPixelPosition.y);
            return interpolatedValue;
        }


        public float SamplePixel(int2 pixel)
        {
            var index = Utils.PositionToIndex(pixel, Size);
            return Data[index];
        }

        public void Dispose()
        {
            UnsafeUtility.Free(Data, Allocator.Temp);
        }
    }

    public static unsafe class UnsafeUtils
    {
        public static TResult* GetDataBuffer<TResult, THeaderType>(THeaderType* asset)
            where THeaderType : unmanaged where TResult : unmanaged
        {
            return (TResult*) asset + (ulong) sizeof(THeaderType);
        }
    }

    public interface IBinaryAsset
    {
    }
}