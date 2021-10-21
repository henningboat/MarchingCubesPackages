namespace henningboat.CubeMarching.TerrainChunkEntitySystem
{
    public enum GeometryInstructionType : byte
    {
        Shape,
        PositionModification,
        DistanceModification,
        Combiner,
        CopyOriginal,
    }
}