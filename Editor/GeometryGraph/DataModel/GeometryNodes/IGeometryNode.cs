using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using henningboat.CubeMarching.PrimitiveBehaviours;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(RuntimeGeometryGraphResolverContext context, GeometryStackData stackData);
    }

    //todo probably rename
    public struct GeometryStackData
    {
        public GeometryGraphProperty Transformation;
        public GeometryGraphProperty Color;
    }
}