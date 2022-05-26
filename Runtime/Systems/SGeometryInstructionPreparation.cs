using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    public partial class SGeometryInstructionPreparation : SystemBase
    {
        private List<GeometryLayerReference> geometryLayerReferences = new List<GeometryLayerReference>();

        private SSetupGeometryLayers _setupSystem;

        protected override void OnCreate()
        {
            _setupSystem = EntityManager.World.GetOrCreateSystem<SSetupGeometryLayers>();
        }

        protected override void OnUpdate()
        {
            var getGeometryInstructionBuffer = GetBufferFromEntity<GeometryInstruction>();

            Dependency = Entities.ForEach((DynamicBuffer<GeometryInstruction> instructions, in CGeometryLayerTag _) =>
                {
                    instructions.Clear();
                })
                .ScheduleParallel(Dependency);

            foreach (var layerReference in _setupSystem.ExistingGeometryLayers)
            {
                var layerEntity = _setupSystem.GetGeometryLayerSingleton(layerReference);
               
                Dependency = Entities.WithSharedComponentFilter(layerReference)
                    .WithAll<GeometryInstruction, CGeometryInstructionSourceTag>().ForEach(
                        (Entity entity) =>
                        {
                            var newElems = getGeometryInstructionBuffer[entity];
            
                            for (int i = 0; i < newElems.Length; i++)
                            {
                                getGeometryInstructionBuffer[layerEntity].Add(newElems[i]);
                            }
            
                        })
                    .Schedule(Dependency);
            }
        }
    }
}