using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.Utils.Containers;

namespace Editor.GeometryGraph.Conversion
{
    public class CombinerInstruction : GeometryGraphInstruction
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty blendFactorProperty;

        public CombinerInstruction(CombinerOperation operation, GeometryGraphProperty blendFactorProperty,
            int currentCombinerDepth) : base(currentCombinerDepth)
        {
            Operation = operation;
            this.blendFactorProperty = blendFactorProperty;
        }

        public override GeometryInstruction GetInstruction()
        {
            var propertyIndexes = new int32();
            //todo
            propertyIndexes[15] = blendFactorProperty.Index;

            return new GeometryInstruction()
            {
                CombinerDepth = Depth,
                GeometryInstructionType = GeometryInstructionType.Combiner,
                PropertyIndexes = propertyIndexes,
                CombinerBlendOperation = Operation
            };
        }
    }
}