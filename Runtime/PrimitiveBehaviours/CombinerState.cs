using henningboat.CubeMarching.GeometryComponents;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class CombinerState
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty BlendValue;

        public CombinerState(CombinerOperation operation, GeometryGraphProperty blendValue)
        {
            Operation = operation;
            BlendValue = blendValue;
        }
    }
}