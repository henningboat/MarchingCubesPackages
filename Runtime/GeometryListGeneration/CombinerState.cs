using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
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