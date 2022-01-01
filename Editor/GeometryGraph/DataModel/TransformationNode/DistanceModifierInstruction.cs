using System.Linq;
using Editor.GeometryGraph.Conversion;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public abstract class DistanceModifierInstruction : GeometryGraphInstruction
    {
        private GeometryGraphProperty[] _properties;
        private CombinerState _combiner;
        protected abstract DistanceModificationType Type { get; }

        protected DistanceModifierInstruction(int depth, CombinerState combiner,
            params GeometryGraphProperty[] properties) : base(depth)
        {
            _combiner = combiner;
            _properties = properties;
        }


        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.DistanceModification,
                (int) Type, _properties.ToList());
        }
    }
}