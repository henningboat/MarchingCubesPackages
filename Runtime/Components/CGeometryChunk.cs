using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryChunk:IComponentData
    {
        public float3 PositionWS;
        public int IndexInIndexMap;
        public GeometryInstructionHash ContentHash;
    }

    public struct CGeometryLayerReference : IComponentData
    {
        public Entity LayerEntity;
    }
}