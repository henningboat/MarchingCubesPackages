using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class SimpleDebugSphere : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private GeometryLayerBehaviour _layer;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var instruction =
                GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType.Sphere,
                    new List<GeometryGraphProperty>());

            var identity = float4x4.identity;
            var properties = new float32();
            properties[0] = 8;
            for (int i = 0; i < 16; i++)
            {
                properties[16 + i] = identity[i / 4][i % 4];
            }

            instruction.ResolvedPropertyValues = properties;

            dstManager.AddBuffer<GeometryInstruction>(entity).Add(instruction);

            var layerEntity = conversionSystem.GetPrimaryEntity(_layer);
            dstManager.AddComponentData(entity, new GeometryLayerReference()
            {
                LayerEntity = layerEntity,
            });
        }
    }

    public struct GeometryLayerReference : IComponentData
    {
        public Entity LayerEntity;
    }
}