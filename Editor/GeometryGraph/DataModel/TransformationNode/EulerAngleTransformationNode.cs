﻿using System;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public class EulerAngleTransformationNode : TransformationNode, IGeometryNode
    {
        public IPortModel InEulerAngles;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            InEulerAngles = this.AddDataInputPort<Vector3>("Rotation", nameof(InEulerAngles));
        }

        protected override GeometryGraphProperty GetTransformationProperty(GeometryInstructionListBuilder context)
        {
            throw new NotImplementedException();
        }
    }
}