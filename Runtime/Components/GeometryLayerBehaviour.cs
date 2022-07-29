﻿using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class GeometryLayerBehaviour : MonoBehaviour,IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new CGeometryLayerInstance());
            dstManager.AddBuffer<GeometryInstruction>(entity);
        }
    }

    public struct CGeometryLayerInstance : IComponentData
    {
        public bool ClearEveryFrame;
        public long Iteration;
    }

    public struct CGeometryLayerChild : IBufferElementData
    {
        public Entity ClusterEntity;
    }
}