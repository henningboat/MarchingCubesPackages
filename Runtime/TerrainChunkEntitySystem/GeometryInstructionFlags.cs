using System;

namespace henningboat.CubeMarching.TerrainChunkEntitySystem
{
    [Flags]
    public enum GeometryInstructionFlags
    {
        None = 0,
        HasMaterial = 1 << 0,
    }
}