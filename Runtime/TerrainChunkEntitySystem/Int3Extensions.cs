using Unity.Mathematics;

namespace TerrainChunkEntitySystem
{
    public static class Int3Extensions
    {
        public static int Volume(this int3 vector)
        {
            return vector.x * vector.y * vector.z;
        }
    }
}