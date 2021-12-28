using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
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


        public void Resolve(RuntimeGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            _geometryInput.ResolveGeometryInput(context, stackData);
            throw new NotImplementedException();
//            context.WriteDistanceModifier(GetDistanceModifierInstruction(context, stackData));
        }

        protected abstract DistanceModifierInstruction GetDistanceModifierInstruction(
            RuntimeGeometryGraphResolverContext geometryGraphResolverContext, GeometryStackData stackData);
    }
}