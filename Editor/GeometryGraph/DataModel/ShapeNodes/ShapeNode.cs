using System;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.ShapeNodes
{
    [Serializable]
    public class ShapeNode : GeometryInstructionBaseNode<ShapeType>
    {
        protected override int GetTypeEnumValue()
        {
            return (int) _typeEnumValue;
        }

        protected override void OnDefineNode()
        {
            if (_typeEnumValue == ShapeType.SDF2D)
            {
                this.AddDataInputPort<Texture2D>("_assetReference", "_assetReference");
            }
            base.OnDefineNode();
        }

        public override GeometryInstructionType InstructionType => GeometryInstructionType.Shape;
    }
}