using System;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public class MathAdditionOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Addition;
    }

    [Serializable]
    public class MathSubtractionOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Subtraction;
    }
}