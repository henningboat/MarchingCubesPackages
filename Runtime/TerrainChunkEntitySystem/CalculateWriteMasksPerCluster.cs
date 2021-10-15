using Rendering;
using Utils;

namespace TerrainChunkEntitySystem
{
    public struct CalculateWriteMasksPerCluster
    {
        public void Execute(ref CClusterParameters clusterParameters)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
        }
    }
}