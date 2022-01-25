using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.DistanceFieldGeneration
{
    [BurstCompile]
    internal struct JCalculateDistanceField : IJobParallelFor
    {
        private GeometryFieldData _geometryFieldData;
        private NativeArray<GeometryInstruction> _geometryInstructions;
        private DistanceDataReadbackCollection _readbackCollection;

        public JCalculateDistanceField(GeometryFieldData geometryFieldData, NativeArray<GeometryInstruction> geometryInstructions, DistanceDataReadbackCollection readbackCollection)
        {
            _geometryInstructions = geometryInstructions;
            _geometryFieldData = geometryFieldData;
            _readbackCollection = readbackCollection;
        }

        public void Execute(int index)
        {
            var clusterIndex = index / Constants.chunksPerCluster;
            var chunkIndexInCluster = index % Constants.chunksPerCluster;

            var cluster = _geometryFieldData.GetCluster(clusterIndex);
            var chunk = cluster.GetChunk(chunkIndexInCluster);

            DistanceFieldResolver.CalculateDistanceFieldForChunk(cluster, chunk, _geometryInstructions,_readbackCollection);
        }
    }
}