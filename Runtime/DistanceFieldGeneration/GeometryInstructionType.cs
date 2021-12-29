namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
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