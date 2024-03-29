﻿using System;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public class PIConstant : MathNode
    {
        public override string Title
        {
            get => "π";
            set { }
        }

        public override void ResetConnections()
        {
        }

        protected override void OnDefineNode()
        {
            this.AddDataOutputPort<float>("");
            this.AddDataInputPort<Vector3>("position");
        }
    }
}