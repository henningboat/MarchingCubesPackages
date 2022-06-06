using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = UnityEngine.Hash128;

namespace henningboat.CubeMarching.Runtime.Components
{
    public class SimpleDebugSphere : MonoBehaviour//, IConvertGameObjectToEntity
    {
        [SerializeField] private GeometryLayerAsset _layer;
        [SerializeField] private GameObject _testPrefab;
        [SerializeField] private Texture _testTexture;
        

        public GeometryLayerAsset layer => _layer;

        public GameObject testPrefab => _testPrefab;

        public Texture testTexture => _testTexture;

        // public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        // {
        //     var instruction =
        //         GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType.Sphere,
        //             new List<GeometryGraphProperty>());
        //
        //     var identity = float4x4.identity;
        //     var properties = new float32();
        //     properties[0] = 3;
        //     for (int i = 0; i < 16; i++)
        //     {
        //         properties[16 + i] = identity[i / 4][i % 4];
        //     }
        //
        //     instruction.ResolvedPropertyValues = properties;
        //
        //     dstManager.AddBuffer<GeometryInstruction>(entity).Add(instruction);
        //     dstManager.AddComponent<CGeometryInstructionSourceTag>(entity);
        //
        //     conversionSystem.DeclareReferencedAsset(_layer);
        //
        //     dstManager.AddSharedComponentData(entity, new GeometryLayerReference(_layer));
        // }
    }

    [ConverterVersion("Henning", 2)]
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class RefereceDebugSphereConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((SimpleDebugSphere sphere) =>
            {
                DeclareAssetDependency(sphere.gameObject, sphere.layer);
            });
        }
    }
    
    [ConverterVersion("Henning", 1)]
    public class SimpleDebugSphereConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((SimpleDebugSphere sphere) =>
            {
               // DeclareReferencedAsset(sphere.layer);
               var entity = GetPrimaryEntity(sphere);
                
               var instruction =
                   GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType.Torus,
                       new List<GeometryGraphProperty>());

               var identity = (float4x4)sphere.transform.worldToLocalMatrix;
               var properties = new float32();
               
               properties[0] = 8f;
               properties[1] = 2f;
               
               for (int i = 0; i < 16; i++)
               {
                   properties[16 + i] = identity[i / 4][i % 4];
               }


               instruction.ResolvedPropertyValues = properties;

               DstEntityManager.AddBuffer<GeometryInstruction>(entity).Add(instruction);
               DstEntityManager.AddComponent<CGeometryInstructionSourceTag>(entity);

               DstEntityManager.AddComponent<CTestComponent>(entity);
               DstEntityManager.SetComponentData(entity, new CTestComponent() {render = sphere.testTexture.imageContentsHash.GetHashCode()});

               DstEntityManager.AddSharedComponentData(entity, new GeometryLayerAssetsReference(sphere.layer));
            });
            
            Entities.ForEach((GeometryLayerAsset asset) =>
            {
                var layerEntityTest = GetPrimaryEntity(asset);
                DstEntityManager.AddComponent<CGeometryLayerTag>(layerEntityTest);
                DstEntityManager.AddComponent<Translation>(layerEntityTest);
            });
        }
    }

    public struct CTestComponent:IComponentData
    {
        public int render;
    }


    public struct CGeometryInstructionSourceTag:IComponentData
    {
    }

    public struct GeometryLayerAssetsReference : ISharedComponentData, IEquatable<GeometryLayerAssetsReference>
    {
        public bool Equals(GeometryLayerAssetsReference other)
        {
            return other.LayerAsset == LayerAsset;
        }

        public override bool Equals(object obj)
        {
            return obj is GeometryLayerAssetsReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return LayerAsset != null ? LayerAsset.GetHashCode() : 0;
        }

        public GeometryLayerAsset LayerAsset;

        public GeometryLayerAssetsReference(GeometryLayerAsset layerAsset)
        {
            LayerAsset = layerAsset;
        }
    }
}