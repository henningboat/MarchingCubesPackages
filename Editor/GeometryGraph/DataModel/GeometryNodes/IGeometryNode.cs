using henningboat.CubeMarching.Runtime.GeometryListGeneration;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(GeometryInstructionListBuilder context);
    }

    //todo probably rename
}