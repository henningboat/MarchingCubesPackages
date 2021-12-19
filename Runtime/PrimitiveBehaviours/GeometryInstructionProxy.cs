using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using UnityEngine;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public abstract class GeometryInstructionProxy
    {
        public GeometryGraphProperty TransformationValue { get; private set; }

        public GeometryInstructionProxy(GeometryGraphProperty transformation)
        {
            TransformationValue = transformation;
        }

        public GeometryInstruction GetGeometryInstruction(RuntimeGeometryGraphResolverContext context)
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType, GeometryInstructionSubType,
                context.CurrentCombinerDepth, context.CurrentCombinerOperation, context.CreateProperty(0.0f),
                TransformationValue, GetProperties(), context.CreateProperty(0.0f));
        }

        protected abstract List<GeometryGraphProperty> GetProperties();

        public abstract int GeometryInstructionSubType { get; }

        public abstract GeometryInstructionType GeometryInstructionType { get; }
    }
}