using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.Utils.Containers;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    public static class GeometryInstructionUtility
    {
        public static GeometryInstruction CreateInstruction(GeometryInstructionType geometryInstructionType,
            int subType, List<GeometryGraphProperty> shapeProperties)
        {
            var propertyIndexes = new int32();

            var propertyOffset = 0;
            if (shapeProperties != null)
                foreach (var shapeProperty in shapeProperties)
                    for (var i = 0; i < shapeProperty.GetSizeInBuffer(); i++)
                    {
                        propertyIndexes[propertyOffset] = shapeProperty.Index + i;
                        propertyOffset++;
                    }

            if (propertyOffset >= 14) throw new ArgumentOutOfRangeException();


            return new GeometryInstruction()
            {
                GeometryInstructionType = geometryInstructionType,
                GeometryInstructionSubType = subType,
                PropertyIndexes = propertyIndexes
            };
        }

        public static void AddAdditionalData(ref GeometryInstruction instruction, int depth,
            CombinerOperation combinerOperation,
            GeometryGraphProperty combinerBlendValue, GeometryGraphProperty transformation, GeometryGraphProperty color,
            int localAssetIndex = -1)
        {
            var propertyIndexes = instruction.PropertyIndexes;

            propertyIndexes[14] = color?.Index ?? 0;
            propertyIndexes[15] = combinerBlendValue.Index;
            
            
            for (var i = 0; i < 16; i++) propertyIndexes[16 + i] = transformation.Index + i;

            instruction.PropertyIndexes = propertyIndexes;


            instruction.CombinerDepth = depth;
            instruction.CombinerBlendOperation = combinerOperation;
            instruction.PropertyIndexes = propertyIndexes;

            instruction.assetReferenceIndex = localAssetIndex;
        }
    }
}