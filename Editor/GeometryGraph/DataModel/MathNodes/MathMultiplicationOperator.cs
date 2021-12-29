using System;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public class MathMultiplicationOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Multiplication;
    }
}