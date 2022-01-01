using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    [UsedImplicitly]
    public class ResetTransformationNode : PositionModificationNode
    {
        private IPortModel _periodInput;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _periodInput = this.AddDataInputPort("", nameof(_periodInput), defaultValue: Vector3.one * 8);
        }

        protected override PositionModificationInstruction GetDistanceModifierInstruction(
            GeometryInstructionListBuilder context)
        {
            return null;
        }

        public override void Resolve(GeometryInstructionListBuilder context)
        {
            var zeroMatrixInstruction = context.CreateProperty(Matrix4x4.identity);
            context.PushTransformation(zeroMatrixInstruction, false);

            _geometryIn.ResolveGeometryInput(context);

            context.PopTransformation();
        }
    }
}