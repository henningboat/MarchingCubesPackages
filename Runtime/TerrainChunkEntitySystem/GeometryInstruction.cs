using System;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes;
using henningboat.CubeMarching.TerrainChunkSystem;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.TerrainChunkEntitySystem
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

        #endregion

        public void AddValueBufferOffset(int valueBufferOffset)
        {
            PropertyIndexes.AddOffset(valueBufferOffset);
        }

        public CGenericTerrainTransformation GetTerrainTransformation()
        {
            return new() {Data = ResolvedPropertyValues,TerrainTransformationType = (TerrainTransformationType) GeometryInstructionSubType};
        }

        public CGenericGeometryShape GetShapeInstruction()
        {
            return new() {Data = ResolvedPropertyValues, ShapeType = (ShapeType) GeometryInstructionSubType};
        }

        public CGenericDistanceModification GetDistanceModificationInstruction()
        {
            return new() {Data = ResolvedPropertyValues, Type = (DistanceModificationType) GeometryInstructionSubType};
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
        public Hash128 GetRelevantHash()
        {
            Hash128 hash=default;
            hash.Append(ref GeometryInstructionType);
            hash.Append(ref GeometryInstructionSubType);
            
            hash.Append(CombinerDepth);

            bool ignoreTransformation = GeometryInstructionType == GeometryInstructionType.Combiner ||
                                        GeometryInstructionType == GeometryInstructionType.DistanceModification;

            if (ignoreTransformation)
            {
                float4x4 propertyData = UnsafeCastHelper.Cast<float32, float4x4>(ref ResolvedPropertyValues);
                hash.Append(ref propertyData);
            }
            else
            {
                hash.Append(ref ResolvedPropertyValues);
            }

            return hash;
        }
    }
}