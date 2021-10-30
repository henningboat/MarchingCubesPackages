using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup;
using Unity.Jobs;

namespace henningboat.CubeMarching.GeometrySystems.MeshGenerationSystem
{
    internal class BuildMainGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphInstance[] _allGeometryGraphInstance;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }

        public JobHandle Update(JobHandle jobHandle)
        {
            //todo initialize in a nicer way
            _allGeometryGraphInstance = UnityEngine.Object.FindObjectsOfType<GeometryGraphInstance>();
            MainRenderGraph = _allGeometryGraphInstance[0].GetGraphData();
            return jobHandle;
        }

        public GeometryGraphData MainRenderGraph { get; private set; }
    }
}