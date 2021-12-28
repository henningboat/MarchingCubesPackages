﻿using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;
using henningboat.CubeMarching.PrimitiveBehaviours;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
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

        protected override GeometryGraphProperty GetTransformationProperty(RuntimeGeometryGraphResolverContext context,
            GeometryGraphProperty parent)
        {
            var translationProperty = _inTranslation.ResolvePropertyInput(context, GeometryPropertyType.Float3);
            var transformationProperty = context.CreateProperty(Matrix4x4.identity);
            //var transformationProperty = new GeometryGraphMathOperatorProperty(GeometryPropertyType.Float4X4, MathOperatorType.Translate, parent, translationProperty, "Translation");

            throw new NotImplementedException();
//            context.WriteMathProperty(MathOperatorType.Translate, parent, translationProperty, transformationProperty);

            return transformationProperty;
        }
    }
}