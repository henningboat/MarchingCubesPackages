using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class OnionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public OnionDistanceModifierInstruction(GeometryGraphProperty thickness,
            RuntimeGeometryGraphResolverContext context, GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Onion;
    }

    public class InflationDistanceModificationInstruction : DistanceModifierInstruction
    {
        public InflationDistanceModificationInstruction(GeometryGraphProperty thickness,
            RuntimeGeometryGraphResolverContext context, GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Inflation;
    }

    public class InversionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public InversionDistanceModifierInstruction(RuntimeGeometryGraphResolverContext context,
            GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Inversion;
    }
}