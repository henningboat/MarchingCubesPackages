using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.Utils.Containers;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public struct CalculateWriteMasksPerCluster
    {
        public void Execute(ref CClusterParameters clusterParameters)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
        }
    }
}