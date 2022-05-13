using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    public partial class SSetupDistanceField : SystemBase
    {
        private EntityArchetype _entityClusterArchetype;

        private static readonly CGeometryFieldSettings Settings = new CGeometryFieldSettings
            {ClusterCounts = new int3(2, 2, 2)};

        protected override void OnCreate()
        {
            _entityClusterArchetype =
                EntityManager.CreateArchetype(typeof(CGeometryCluster), typeof(PackedDistanceFieldData));
        }


        protected override void OnUpdate()
        {
            //Delete Clusters of destroyed geometry layers
            Entities.WithNone<CGeometryLayer>().WithAll<CGeometryLayerChild>().ForEach(
                    (Entity layerEntity, DynamicBuffer<CGeometryLayerChild> childEntityBuffer) =>
                    {
                        EntityManager.DestroyEntity(childEntityBuffer.Reinterpret<Entity>().AsNativeArray());
                        EntityManager.RemoveComponent<CGeometryLayerChild>(layerEntity);
                    })
                .WithStructuralChanges().Run();


            //Generate Clusters for all new geometry layers
            Entities.WithAll<CGeometryLayer>().WithNone<CGeometryLayerChild>().ForEach((Entity layerEntity) =>
            {
                var clusterCount = Settings.ClusterCounts.Volume();

                var clusterEntities = EntityManager.CreateEntity(_entityClusterArchetype, clusterCount, Allocator.Temp);
                var childBuffer = EntityManager.AddBuffer<CGeometryLayerChild>(layerEntity);
                childBuffer.AddRange(clusterEntities.Reinterpret<CGeometryLayerChild>());

                for (var i = 0; i < clusterEntities.Length; i++)
                {
                    var position = DistanceFieldGeneration.Utils.IndexToPositionWS(i, Settings.ClusterCounts) *
                                   Constants.chunkLength;

                    var clusterEntity = clusterEntities[i];
                    EntityManager.SetName(clusterEntity, $"Cluster {position.ToString()}");
                    EntityManager.SetComponentData(clusterEntity, new CGeometryCluster {PositionWS = position});
                    var distanceFieldDatas = EntityManager.GetBuffer<PackedDistanceFieldData>(clusterEntity);
                    distanceFieldDatas.Length = Constants.chunkVolume;
                }
            }).WithStructuralChanges().Run();
        }
    }
}