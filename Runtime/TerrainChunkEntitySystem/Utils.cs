using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace henningboat.CubeMarching.TerrainChunkEntitySystem
{
    public static class Utils
    {
        #region Static Stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToPositionWS(int i, int3 size)
        {
            var index = i;

            var x = index % size.x;
            var y = index / size.x % size.y;
            var z = index / (size.x * size.y);

            return new int3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int3 position, int3 size)
        {
            return position.x + position.y * size.x + position.z * size.x * size.y;
        }

        #endregion
    }
}
