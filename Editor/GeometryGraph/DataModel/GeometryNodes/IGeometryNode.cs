using Code.CubeMarching.GeometryGraph.Editor.Conversion;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(EditorGeometryGraphResolverContext context, GeometryStackData stackData);
    }

    //todo probably rename
    public struct GeometryStackData
    {
        public GeometryGraphProperty Transformation;
        public GeometryGraphProperty Color;
    }
}