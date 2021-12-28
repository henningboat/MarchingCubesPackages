using henningboat.CubeMarching.PrimitiveBehaviours;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData);
    }

    //todo probably rename
    public struct GeometryStackData
    {
        public GeometryGraphProperty Transformation;
        public GeometryGraphProperty Color;
    }
}