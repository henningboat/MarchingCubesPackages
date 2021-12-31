using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    public class CombinerState
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty BlendValue;
        public GeometryGraphProperty Transformation;

        public CombinerState(CombinerOperation operation, GeometryGraphProperty blendValue,
            GeometryGraphProperty transformation)
        {
            Operation = operation;
            BlendValue = blendValue;
            this.Transformation = transformation;
        }
    }
}