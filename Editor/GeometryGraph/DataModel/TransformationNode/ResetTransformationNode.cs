﻿using Code.CubeMarching.GeometryGraph.Editor.Conversion;
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
            EditorGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return null;
        }

        public override void Resolve(EditorGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            var zeroMatrixInstruction = context.GetOrCreateProperty(SerializableGUID.Generate(),Matrix4x4.identity);
            stackData.Transformation = zeroMatrixInstruction;

            _geometryIn.ResolveGeometryInput(context, stackData);
        }
    }
}