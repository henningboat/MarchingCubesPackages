using System;
using Authoring;
using GeometryComponents;

namespace TerrainChunkEntitySystem
{
    [Serializable]
    public struct GeometryInstruction
    {
        #region Public Fields

        public int CombinerDepth;
        public GeometryInstructionType GeometryInstructionType;
        
        public int GeometryInstructionSubType;
        
        public int16 PropertyIndexes;
        public Float4X4Value TransformationValue;
        
        public CGeometryCombiner Combiner;

        public GeometryInstructionFlags _flags;
        public bool HasMaterial => (_flags & GeometryInstructionFlags.HasMaterial) != 0;
        public MaterialDataValue MaterialData;
        public bool WritesToDistanceField => GeometryInstructionType != GeometryInstructionType.PositionModification;
        
        #endregion

        public void AddValueBufferOffset(int valueBufferOffset)
        {
            PropertyIndexes.AddOffset(valueBufferOffset);
            TransformationValue.Index += valueBufferOffset;
        }

        public CGenericTerrainTransformation GetTerrainTransformation()
        {
            return new() {Data = PropertyIndexes,TerrainTransformationType = (TerrainTransformationType) GeometryInstructionSubType};
        }

        public CGenericGeometryShape GetShapeInstruction()
        {
            return new() {Data = PropertyIndexes, ShapeType = (ShapeType) GeometryInstructionSubType};
        }

        public CGenericDistanceModification GetDistanceModificationInstruction()
        {
            return new() {Data = PropertyIndexes, Type = (DistanceModificationType) GeometryInstructionSubType};
        }
    }
}