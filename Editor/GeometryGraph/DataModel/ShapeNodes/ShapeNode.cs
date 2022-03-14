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
        public override GeometryInstructionType InstructionType => GeometryInstructionType.Shape;
    }
}