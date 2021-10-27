﻿using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using henningboat.CubeMarching;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
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

        protected override DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return new OnionDistanceModifierInstruction(_thicknessInput.ResolvePropertyInput(context, GeometryPropertyType.Float), context,stackData);
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

        protected override DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return new OnionDistanceModifierInstruction(_amountInput.ResolvePropertyInput(context, GeometryPropertyType.Float), context, stackData);
        }
    }
    
    
    [UsedImplicitly]
    public class InversionNode : DistanceModificationNode
    {
        protected override DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return new InversionDistanceModifierInstruction(context, stackData);
        }
    }
}