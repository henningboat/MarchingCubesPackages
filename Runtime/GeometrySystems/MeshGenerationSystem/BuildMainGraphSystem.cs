using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    internal class BuildMainGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphInstance[] _allGeometryGraphInstances;
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;

            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.Persistent);
        }

        public JobHandle Update(JobHandle jobHandle)
        {
            //todo placeholder
            jobHandle.Complete();
        
            //todo initialize in a nicer way
            _allGeometryGraphInstances = UnityEngine.Object.FindObjectsOfType<GeometryGraphInstance>();

            _allGeometryInstructionsList.Clear();

            int offset = 0;
            //could be turned into a job, but doesnt cost much right now
            foreach (GeometryGraphInstance graphInstance in _allGeometryGraphInstances)
            {
                _allGeometryInstructionsList.AddRange(graphInstance.GraphData.GeometryInstructions);
            }

            MainRenderGraph = _allGeometryInstructionsList.AsArray();

            return jobHandle;
        }

        public void Dispose()
        {
            _allGeometryInstructionsList.Dispose();
        }

        public NativeArray<GeometryInstruction> MainRenderGraph { get; private set; } = default;
    }

    public static class NativeArrayExtensions
    {
        public static void CopyFrom<T>(this NativeArray<T> target, NativeArray<T> source, int targetStartIndex,
            int sourceStartIndex, int count) where T : struct
        {
            var targetSlice = target.Slice(targetStartIndex, count);
            var sourceSlice = source.Slice(sourceStartIndex, count);

            targetSlice.CopyFrom(sourceSlice);
        }
    }
}