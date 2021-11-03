using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
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
            GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return null;
        }

        public override void Resolve(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            var zeroMatrixInstruction = context.GetOrCreateProperty(SerializableGUID.Generate(),
                new GeometryGraphConstantProperty(Matrix4x4.identity, context, GeometryPropertyType.Float4X4,
                    "Identity Transformation"));
            stackData.Transformation = zeroMatrixInstruction;

            _geometryIn.ResolveGeometryInput(context, stackData);
        }
    }
}