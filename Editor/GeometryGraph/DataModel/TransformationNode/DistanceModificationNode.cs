using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public abstract class DistanceModificationNode : GeometryNodeModel, IGeometryNode
    {
        private IPortModel _geometryInput;
        private IPortModel _geometryOutput;

        protected override void OnDefineNode()
        {
            _geometryInput = AddExecutionInput(nameof(_geometryInput));
            _geometryOutput = AddExecutionOutput(nameof(_geometryInput));
        }


        public void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData)
        {
            _geometryInput.ResolveGeometryInput(context, stackData);
            throw new NotImplementedException();
//            context.WriteDistanceModifier(GetDistanceModifierInstruction(context, stackData));
        }

        protected abstract DistanceModifierInstruction GetDistanceModifierInstruction(
            GeometryInstructionListBuilder geometryInstructionListBuilder, GeometryStackData stackData);
    }
}