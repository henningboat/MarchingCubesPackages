﻿using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathOperator : MathNode
    {
        [SerializeField] [HideInInspector] private int m_InputPortCount = 2;

        public abstract MathOperatorType OperatorType { get; }

        public override void ResetConnections()
        {
        }

        public override string Title
        {
            get => OperatorType.ToString();
            set { }
        }

        public int InputPortCount
        {
            get => m_InputPortCount;
            set => m_InputPortCount = Math.Max(2, value);
        }

        protected void AddInputPorts()
        {
            for (var i = 0; i < InputPortCount; ++i) this.AddDataInputPort<float>("Term " + (i + 1));
        }

        public IPortModel DataOut { get; private set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            DataOut = this.AddDataOutputPort<float>("Output");

            AddInputPorts();
        }


        public GeometryGraphProperty[] GetInputProperties(GeometryInstructionListBuilder context,
            GeometryPropertyType propertyType)
        {
            var inputProperties = new List<GeometryGraphProperty>();

            var inputs = this.GetInputPorts().ToList();

            for (var i = 0; i < inputs.Count; i++)
                inputProperties.Add(inputs[i].ResolvePropertyInput(context, propertyType));

            return inputProperties.ToArray();
        }
    }
}