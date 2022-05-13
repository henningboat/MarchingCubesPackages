using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    public partial class SGeometryInstructionPreparation : SystemBase
    {
        protected override void OnUpdate()
        {
            var getGeometryInstructionBuffer = GetBufferFromEntity<GeometryInstruction>(false);

            Dependency = Entities.ForEach((DynamicBuffer<GeometryInstruction> instructions, in CGeometryLayer _) =>
                {
                    instructions.Clear();
                })
                .ScheduleParallel(Dependency);

            Dependency = Entities.WithAll<GeometryInstruction, GeometryLayerReference>().ForEach(
                    (Entity entity, in GeometryLayerReference geometryLayerReference) =>
                    {
                        var newElems = getGeometryInstructionBuffer[entity];

                        for (int i = 0; i < newElems.Length; i++)
                        {
                            getGeometryInstructionBuffer[geometryLayerReference.LayerEntity]
                                .Add(newElems[i]);
                        }

                    }).WithNativeDisableParallelForRestriction(getGeometryInstructionBuffer)
                .Schedule(Dependency);
        }
    }
}