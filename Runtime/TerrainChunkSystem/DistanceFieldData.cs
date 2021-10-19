using System.Runtime.InteropServices;

namespace TerrainChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DistanceFieldData
    {
        public float SurfaceDistance;
        public TerrainMaterial TerrainMaterial;

        public static readonly DistanceFieldData DefaultOutside = new()
        {
            SurfaceDistance = 10
        };
    }
}