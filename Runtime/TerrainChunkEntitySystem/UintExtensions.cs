using Unity.Mathematics;

namespace TerrainChunkEntitySystem
{
    public static class UintExtensions
    {
        public static void AddToHash(ref this uint a, uint b)
        {
            a = math.hash(new uint2(a, b));
        }
    }
}