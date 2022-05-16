using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class GeometryLayerBehaviour : MonoBehaviour,IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new CGeometryLayerTag());
            dstManager.AddBuffer<GeometryInstruction>(entity);
        }
    }

    public struct CGeometryLayerTag : IComponentData
    {
        
    }

    public struct CGeometryLayerChild : IBufferElementData
    {
        public Entity ClusterEntity;
    }
}