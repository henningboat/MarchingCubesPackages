using Unity.Entities;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryLayerListElement : IBufferElementData
    {
        public SerializableGUID LayerID;
        public Entity InstructionListHandler;
    }
}