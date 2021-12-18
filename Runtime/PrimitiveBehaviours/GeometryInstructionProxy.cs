using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using UnityEngine;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public abstract class GeometryInstructionProxy
    {
        public GeometryInstruction GetGeometryInstruction(RuntimeGeometryGraphResolverContext context)
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType, GeometryInstructionSubType,
                context.CurrentCombinerDepth, context.CurrentCombinerOperation, context.Constant(0),
                context.Constant(Matrix4x4.identity), GetProperties(), context.Constant(0));
        }

        protected abstract List<GeometryGraphValue> GetProperties();

        public abstract int GeometryInstructionSubType { get; }

        public abstract GeometryInstructionType GeometryInstructionType { get; }
    }
}