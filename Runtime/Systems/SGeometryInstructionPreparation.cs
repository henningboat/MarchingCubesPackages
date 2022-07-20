using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryGraphPreparation;
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


            Dependency = Entities.WithNone<CGeometryLayerTag>().ForEach(
                (ref DynamicBuffer<GeometryInstruction> geometryInstructions) =>
                {
                    for (var i = 0; i < geometryInstructions.Length; i++)
                    {
                        var instruction = geometryInstructions[i];
                        instruction.UpdateHash();
                        geometryInstructions[i] = instruction;
                    }
                }).WithBurst().ScheduleParallel(Dependency);

            
            var getGeometryInstructionBuffer = GetBufferFromEntity<GeometryInstruction>();

            Dependency = Entities.ForEach((DynamicBuffer<GeometryInstruction> instructions, in CGeometryLayerTag _) =>
                {
                    instructions.Clear();
                }).WithBurst()
                .ScheduleParallel(Dependency);

            
            var layerList = _setupSystem.GeometryLayerList;

            foreach (var layerReference in _setupSystem.ExistingGeometryLayers)
            {
                var layerEntity = _setupSystem.GetGeometryLayerSingleton<CGeometryLayerTag>(layerReference);
                Dependency = Entities.WithSharedComponentFilter(layerReference)
                    .WithAll<GeometryInstruction, CGeometryInstructionSourceTag>().ForEach(
                        (Entity entity) =>
                        {
                            CopyInstructionsFromEntity(entity, layerEntity, getGeometryInstructionBuffer, layerList, 0);
                        })
                    .WithBurst().Schedule(Dependency);
            }
        }

        private static void CopyInstructionsFromEntity(Entity source, Entity target,
            BufferFromEntity<GeometryInstruction> geometryInstructionBuffer,
            DynamicBuffer<CGeometryLayerListElement> layerList, int stackOffset)
        {
            var newElems = geometryInstructionBuffer[source];
 
            for (var i = 0; i < newElems.Length; i++) 
            {
                var instruction = newElems[i];
                
                if (instruction.GeometryInstructionType == GeometryInstructionType.CopyLayer)
                {
                    CGeometryLayerListElement correctLayer = default;
                    for (var j = 0; j < layerList.Length; j++)
                        if (layerList[j].LayerID == instruction.ReferenceGUID)
                        {
                            correctLayer = layerList[j];
                            CopyInstructionsFromEntity(correctLayer.InstructionListHandler, target, geometryInstructionBuffer,
                                layerList, instruction.CombinerDepth);
                            break;
                        }
                }
                else
                {
                    instruction.CombinerDepth += stackOffset;
                    geometryInstructionBuffer[target].Add(instruction);
                }
            }
        }
    } 
}  