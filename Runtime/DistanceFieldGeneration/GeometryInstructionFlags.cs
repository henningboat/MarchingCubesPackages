using System;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    [Flags]
    public enum GeometryInstructionFlags
    {
        None = 0,
        HasMaterial = 1 << 0,
    }
}