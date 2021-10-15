using TerrainChunkEntitySystem;
using Unity.Entities;

public struct GeometryGraphBlob
{
    public BlobArray<CGeometryGraphPropertyValue> valueBuffer;
    public BlobArray<MathInstruction> mathInstructions;
    public BlobArray<GeometryInstruction> geometryInstructions;
}