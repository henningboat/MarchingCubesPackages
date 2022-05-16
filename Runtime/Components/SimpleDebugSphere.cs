using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class SimpleDebugSphere : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private GeometryLayerAsset _layer;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var instruction =
                GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType.Sphere,
                    new List<GeometryGraphProperty>());

            var identity = float4x4.identity;
            var properties = new float32();
            properties[0] = 3;
            for (int i = 0; i < 16; i++)
            {
                properties[16 + i] = identity[i / 4][i % 4];
            }

            instruction.ResolvedPropertyValues = properties;

            dstManager.AddBuffer<GeometryInstruction>(entity).Add(instruction);
            dstManager.AddComponent<CGeometryInstructionSourceTag>(entity);

            dstManager.AddSharedComponentData(entity, new GeometryLayerReference(_layer));
        }
    }

    public struct CGeometryInstructionSourceTag:IComponentData
    {
    }

    public struct GeometryLayerReference : ISharedComponentData, IEquatable<GeometryLayerReference>
    {
        public bool Equals(GeometryLayerReference other)
        {
            return other.LayerAsset == LayerAsset;
        }

        public override bool Equals(object obj)
        {
            return obj is GeometryLayerReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return LayerAsset != null ? LayerAsset.GetHashCode() : 0;
        }

        public GeometryLayerAsset LayerAsset;

        public GeometryLayerReference(GeometryLayerAsset layerAsset)
        {
            LayerAsset = layerAsset;
        }
    }
}