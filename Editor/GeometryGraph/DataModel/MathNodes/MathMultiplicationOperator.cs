using System;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public class MathMultiplicationOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Multiplication;
    }
}