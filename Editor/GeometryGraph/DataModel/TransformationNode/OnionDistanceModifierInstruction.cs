using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class OnionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public OnionDistanceModifierInstruction(GeometryGraphProperty thickness, GeometryGraphResolverContext context, GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Onion;
    }
}