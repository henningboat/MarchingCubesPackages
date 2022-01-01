﻿using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    [UsedImplicitly]
    public class OnioningNode : DistanceModificationNode
    {
        private IPortModel _thicknessInput;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _thicknessInput = this.AddDataInputPort<float>("Thickness", nameof(_thicknessInput), defaultValue: 2);
        }

        protected override DistanceModifierInstruction GetDistanceModifierInstruction(
            GeometryInstructionListBuilder context)
        {
            return new OnionDistanceModifierInstruction(
                _thicknessInput.ResolvePropertyInput(context, GeometryPropertyType.Float), context);
        }
    }


    [UsedImplicitly]
    public class InflationNode : DistanceModificationNode
    {
        private IPortModel _amountInput;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _amountInput = this.AddDataInputPort<float>("Amount", nameof(_amountInput));
        }

        protected override DistanceModifierInstruction GetDistanceModifierInstruction(
            GeometryInstructionListBuilder context)
        {
            return new InflationDistanceModificationInstruction(
                _amountInput.ResolvePropertyInput(context, GeometryPropertyType.Float), context);
        }
    }


    [UsedImplicitly]
    public class InversionNode : DistanceModificationNode
    {
        protected override DistanceModifierInstruction GetDistanceModifierInstruction(
            GeometryInstructionListBuilder context)
        {
            return new InversionDistanceModifierInstruction(context);
        }
    }
}