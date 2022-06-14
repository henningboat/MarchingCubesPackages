using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    public partial class SGeometryInstructionPreparation : SystemBase
    {
        private SSetupGeometryLayers _setupSystem;

        protected override void OnCreate()
        {
            _setupSystem = EntityManager.World.GetOrCreateSystem<SSetupGeometryLayers>();
        }

        protected override void OnUpdate()
        {
            Dependency=Entities.ForEach((ref DynamicBuffer<CGeometryGraphValue> graphValue,
                in LocalToWorld localToWorld, in CMainTransformationIndex mainTransformationIndex) =>
            {
                var valueNativeArray = graphValue.Reinterpret<float>().AsNativeArray();
                var worldToLocal = math.fastinverse(localToWorld.Value);

                for (int i = 0; i < 16; i++)
                {
                    valueNativeArray[i + mainTransformationIndex.Index] = worldToLocal[i / 4][i % 4];
                }

            }).WithBurst().ScheduleParallel(Dependency);

            
            Dependency=Entities.ForEach((ref DynamicBuffer<CMathInstruction> mathInstructions,
                ref DynamicBuffer<CGeometryGraphValue> graphValue, in LocalToWorld localToWorld) =>
            {
                var valueNativeArray = graphValue.Reinterpret<float>().AsNativeArray();
                
                foreach (var mathInstruction in mathInstructions)
                {
                    mathInstruction.Execute(valueNativeArray);
                }
                
            }).WithBurst().ScheduleParallel(Dependency);

             
            Dependency=Entities.ForEach((ref DynamicBuffer<CGeometryGraphValue> properties,
                ref DynamicBuffer<GeometryInstruction> geometryInstructions) =>
            {
                for (int instructionIndex = 0; instructionIndex < geometryInstructions.Length; instructionIndex++)
                {
                    var instruction = geometryInstructions[instructionIndex];
                    for (int i = 0; i < 32; i++)
                    {
                        instruction.ResolvedPropertyValues[i] = properties[instruction.PropertyIndexes[i]].Value;
                    }

                    geometryInstructions[instructionIndex] = instruction;
                }
            }).WithBurst().ScheduleParallel(Dependency);
            
            
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