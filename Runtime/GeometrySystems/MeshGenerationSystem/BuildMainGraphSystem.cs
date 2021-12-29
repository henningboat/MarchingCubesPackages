using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using Unity.Collections;
using Unity.Jobs;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    internal class BuildMainGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private NativeList<GeometryInstruction> _allGeometryInstructionsList;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;

            _allGeometryInstructionsList = new NativeList<GeometryInstruction>(Allocator.Persistent);
        }

        public JobHandle Update(JobHandle jobHandle, List<GeometryGraphBuffers> geometryGraphBuffersList)
        {
            //todo placeholder
            jobHandle.Complete();

            _allGeometryInstructionsList.Clear();

            int offset = 0;
            //could be turned into a job, but doesnt cost much right now
            foreach (var graphInstance in geometryGraphBuffersList)
            {
                _allGeometryInstructionsList.AddRange(graphInstance.GeometryInstructions);
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