using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile(FloatPrecision.Low,FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    internal struct JCalculateDistanceField : IJobParallelFor
    {
        private NativeArray<GeometryInstruction> _geometryInstructions;
        private DistanceDataReadbackCollection _readbackCollection;
        private int _geometryLayerIndex;

        public JCalculateDistanceField(int geometryLayerIndex, NativeArray<GeometryInstruction> geometryInstructions, DistanceDataReadbackCollection readbackCollection)
        {
            _geometryLayerIndex = geometryLayerIndex;
            _geometryInstructions = geometryInstructions;
            _readbackCollection = readbackCollection;
        }

        public void Execute(int index)
        {
            var clusterIndex = index / Constants.chunksPerCluster;
            var chunkIndexInCluster = index % Constants.chunksPerCluster;

            var cluster = _readbackCollection[_geometryLayerIndex].GetCluster(clusterIndex);
            var chunk = cluster.GetChunk(chunkIndexInCluster);

            //DistanceFieldResolver.CalculateDistanceFieldForChunk(cluster, chunk, _geometryInstructions,_readbackCollection);
        }
    }
}