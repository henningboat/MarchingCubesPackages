using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    [UsedImplicitly]
    public class TranslationNode : TransformationNode, IGeometryNode
    {
        private IPortModel _inTranslation;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            _inTranslation = this.AddDataInputPort<Vector3>("Translation", nameof(_inTranslation));
        }

        protected override GeometryGraphProperty GetTransformationProperty(GeometryInstructionListBuilder context)
        {
            var translationProperty = _inTranslation.ResolvePropertyInput(context, GeometryPropertyType.Float3);
            context.AddMathInstruction(MathOperatorType.Translate, GeometryPropertyType.Float4X4,
                context.OriginTransformation,
                translationProperty, out var transformationProperty);
            return transformationProperty;
        }
    }
}