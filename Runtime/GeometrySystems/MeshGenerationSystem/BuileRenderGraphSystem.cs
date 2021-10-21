using GeometrySystems.GeometryFieldSetup;
using NonECSImplementation;
using Unity.Jobs;

namespace GeometrySystems.MeshGenerationSystem
{
    internal class BuileRenderGraphSystem
    {
        private GeometryFieldData _geometryFieldData;
        private GeometryGraphInstance[] _allGeometryGraphInstance;

        public void Initialize(GeometryFieldData geometryFieldData)
        {
            _geometryFieldData = geometryFieldData;
        }

        public JobHandle Update()
        {
            //todo initialize in a nicer way
            _allGeometryGraphInstance = UnityEngine.Object.FindObjectsOfType<GeometryGraphInstance>();
            MainRenderGraph = _allGeometryGraphInstance[0].GraphData;
            return default;
        }

        public GeometryGraphData MainRenderGraph { get; private set; }
    }
}