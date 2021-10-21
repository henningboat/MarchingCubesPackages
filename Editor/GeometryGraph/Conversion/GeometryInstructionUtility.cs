using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal static class GeometryInstructionUtility
    {
        public static GeometryInstruction CreateInstruction(GeometryInstructionType geometryInstructionType, int subType, int depth, CGeometryCombiner combiner, GeometryGraphProperty transformation,
            List<GeometryGraphProperty> shapeProperties, GeometryGraphProperty color)
        {
            var transformationValue = new Float4X4Value() {Index = transformation.Index};
            var propertyIndexes = new int16();
           
            for (int i = 0; i < shapeProperties.Count; i++)
             {
                 propertyIndexes[i] = shapeProperties[i].Index;
             }

            var material = new MaterialDataValue()
            {
                Index = color?.Index ?? 0
            };

            return new GeometryInstruction()
            {
                GeometryInstructionType = geometryInstructionType,
                GeometryInstructionSubType = subType,
                Combiner = combiner,
                CombinerDepth = depth,
                TransformationValue = transformationValue,
                PropertyIndexes = propertyIndexes,
                //HasMaterial = color != null,
                MaterialData = material,
            };
        }
    }
}