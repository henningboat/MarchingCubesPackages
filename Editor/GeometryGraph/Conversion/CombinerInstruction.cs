using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
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