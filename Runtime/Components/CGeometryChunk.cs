using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryChunk:IComponentData
    {
        public float3 PositionWS;
        public int IndexInIndexMap;
    }

    public struct CGeometryChunkState : IComponentData
    {
        public bool HasContent;
        /// <summary>
        /// If a chunk contains no content, it might either be fully inside or fully outside of
        /// the geometry
        /// </summary>
        public bool IsFullyInsideGeometry;
        public GeometryInstructionHash ContentHash;
        public bool IsDirty;
    }

    public struct CGeometryLayerReference : IComponentData
    {
        public Entity LayerEntity;
    }
}