using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public class OnionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public OnionDistanceModifierInstruction(GeometryGraphProperty thickness,
            GeometryInstructionListBuilder context, GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Onion;
    }

    public class InflationDistanceModificationInstruction : DistanceModifierInstruction
    {
        public InflationDistanceModificationInstruction(GeometryGraphProperty thickness,
            GeometryInstructionListBuilder context, GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Inflation;
    }

    public class InversionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public InversionDistanceModifierInstruction(GeometryInstructionListBuilder context,
            GeometryStackData stackData) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Inversion;
    }
}