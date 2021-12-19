using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometrySystems.GeometryGraphPreparation;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public class GeometryGraphConstantProperty : GeometryGraphProperty
    {
        public GeometryGraphConstantProperty(int index, object objectValue,GeometryPropertyType geometryPropertyType,string debugInformation="") : base(index, geometryPropertyType,
            debugInformation)
        {
            Value = objectValue;
        }
    }

    public class GeometryGraphMathOperatorProperty : GeometryGraphProperty
    {
        public MathOperatorType OperatorType { get; }
        public GeometryGraphProperty A { get; }
        public GeometryGraphProperty B { get; }

        public GeometryGraphMathOperatorProperty(GeometryPropertyType type, MathOperatorType operatorType, GeometryGraphProperty a, GeometryGraphProperty b,
            string debugInformation) : base(default,type, debugInformation)
        {
            OperatorType = operatorType;
            A = a;
            B = b;
        }

        public MathInstruction GetMathInstruction()
        {
            return new()
            {
                MathOperationType = OperatorType,
                InputAIndex = A.Index,
                InputAType = A.Type,
                InputBIndex = B?.Index ?? 0,
                InputBType = B?.Type ?? default,
                ResultIndex = Index,
                ResultType = Type,
            };
        }
    }
}