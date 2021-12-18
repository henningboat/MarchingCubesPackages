using System;
using System.Collections.Generic;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal static class GeometryInstructionUtility
    {
        public static GeometryInstruction CreateInstruction(GeometryInstructionType geometryInstructionType, int subType,
            int depth, CombinerOperation combinerOperation, GeometryGraphValue combinerBlendValue, GeometryGraphValue transformation,
            List<GeometryGraphValue> shapeProperties, GeometryGraphValue color)
        {
            var propertyIndexes = new int32();

            int propertyOffset = 0;
            foreach (var shapeProperty in shapeProperties)
            {
                for (int i = 0; i < shapeProperty.Size; i++)
                {
                    propertyIndexes[propertyOffset] = shapeProperty.Index + i;
                    propertyOffset++;
                }
            }

            if (propertyOffset >= 14)
            {
                throw new ArgumentOutOfRangeException();
            }

            //14 is used for the material color
            propertyIndexes[14] = color?.Index ?? 0;
            propertyIndexes[15] = combinerBlendValue.Index;

            for (int i = 0; i < 16; i++)
            {
                propertyIndexes[16 + i] = transformation.Index + i;
            }

            return new GeometryInstruction()
            {
                GeometryInstructionType = geometryInstructionType,
                GeometryInstructionSubType = subType,
                CombinerDepth = depth,
                CombinerBlendOperation = combinerOperation,
                PropertyIndexes = propertyIndexes,
                //HasMaterial = color != null,
            };
        }
    }
}