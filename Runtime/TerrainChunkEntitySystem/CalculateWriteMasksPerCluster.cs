using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Utils;

namespace henningboat.CubeMarching.TerrainChunkEntitySystem
{
    public struct CalculateWriteMasksPerCluster
    {
        public void Execute(ref CClusterParameters clusterParameters)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
        }
    }
}