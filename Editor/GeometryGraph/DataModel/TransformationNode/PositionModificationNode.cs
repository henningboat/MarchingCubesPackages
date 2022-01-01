using Editor.GeometryGraph.DataModel.ShapeNodes;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public sealed class PositionModificationNode : GeometryInstructionBaseNode<PositionModificationType>
    {
        private IPortModel _geometryIn;

        protected override int GetTypeEnumValue()
        {
            return (int) _typeEnumValue;
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _geometryIn = AddExecutionInput(nameof(_geometryIn));
        }

        public override GeometryInstructionType InstructionType => GeometryInstructionType.PositionModification;

        public override void Resolve(GeometryInstructionListBuilder context)
        {
            context.PushCombiner(CombinerOperation.Min,context.ZeroFloatProperty);
            base.Resolve(context);
            context.PushTransformation(context.ZeroTransformation, false);
            _geometryIn.ResolveGeometryInput(context);
            context.PopTransformation();
            context.PopCombiner();
        }
    }
}