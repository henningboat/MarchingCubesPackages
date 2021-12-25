using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public abstract class ShapeProxy : GeometryInstructionProxy
    {
        public abstract ShapeType ShapeType { get; }
        public override int GeometryInstructionSubType => (int) ShapeType;
        public override GeometryInstructionType GeometryInstructionType => GeometryInstructionType.Shape;

        protected ShapeProxy(GeometryGraphProperty transformation) : base(transformation)
        {
        }
    }
}