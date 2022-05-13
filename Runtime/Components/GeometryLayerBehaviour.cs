using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class GeometryLayerBehaviour : MonoBehaviour,IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new CGeometryLayer());
            dstManager.AddBuffer<GeometryInstruction>(entity);
        }
    }

    public struct CGeometryLayer : IComponentData
    {
        
    }

    public struct CGeometryLayerChild : ISystemStateBufferElementData
    {
        public Entity ClusterEntity;
    }
}