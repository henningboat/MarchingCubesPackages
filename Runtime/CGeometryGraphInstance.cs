using Unity.Entities;

public struct CGeometryGraphInstance : IComponentData
{
    public BlobAssetReference<GeometryGraphBlob> graph;
}