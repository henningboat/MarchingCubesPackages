using System;

namespace TerrainChunkEntitySystem
{
    [Flags]
    public enum GeometryInstructionFlags
    {
        None = 0,
        HasMaterial = 1 << 0,
    }
}