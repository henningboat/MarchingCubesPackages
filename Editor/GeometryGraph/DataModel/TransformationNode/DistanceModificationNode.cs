using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using Editor.GeometryGraph.DataModel.ShapeNodes;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public sealed class DistanceModificationNode : GeometryInstructionBaseNode<DistanceModificationType>
    {
        private IPortModel _geometryIn;

        protected override int GetTypeEnumValue()
        {
            return (int) _typeEnumValue;
        }

        protected override void OnDefineNode()
        {
            _geometryIn = AddExecutionInput(nameof(_geometryIn));
            base.OnDefineNode();
        }

        public override GeometryInstructionType InstructionType => GeometryInstructionType.DistanceModification;

        public override void Resolve(GeometryInstructionListBuilder context)
        {
            _geometryIn.ResolveGeometryInput(context);
            base.Resolve(context);
        }
    }
}