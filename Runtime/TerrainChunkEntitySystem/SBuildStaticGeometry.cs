﻿

namespace TerrainChunkEntitySystem
{
    // [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    // [UpdateAfter(typeof(SCombineGeometrySubGraphs))]
    // public class SBuildStaticGeometry : SystemBase
    // {
    //     #region Protected methods
    //
    //     protected override void OnUpdate()
    //     {
    //         var terrainChunkBuffer = this.GetSingletonBuffer<TerrainChunkDataBuffer>();
    //         var isPlaying = Application.isPlaying && UnityEngine.Time.frameCount > 1;
    //         var getClusterParameters = GetComponentDataFromEntity<CClusterParameters>(true);
    //         //var hasher = new GeometryInstructionsHasher(this);
    //
    //         var frameCount = GetSingleton<CFrameCount>().Value;
    //         
    //         //Static geometry 
    //         {
    //             //Write Instructions
    //             var mainGraphSingleton = GetSingletonEntity<CMainGeometryGraphSingleton>();
    //
    //             Dependency = Entities
    //                 .ForEach((ref CClusterParameters clusterParameters) =>
    //                 {
    //                     clusterParameters.WriteMask=BitArray512.AllBitsTrue;
    //                 }).WithName("WriteClusterWriteMasks").WithBurst().Schedule(Dependency);
    //
    //             var getValueBuffer = GetBufferFromEntity<CGeometryGraphPropertyValue>(true);
    //             //Calculate Distance Fields
    //             var getTerrainInstructionBuffer = GetBufferFromEntity<GeometryInstruction>(true);
    //
    //             Dependency = Entities.ForEach((ref CTerrainChunkDynamicData distanceField, in ClusterChild clusterChild,
    //                     in CTerrainEntityChunkPosition chunkPosition) =>
    //                 {
    //                     var clusterParameters = getClusterParameters[clusterChild.ClusterEntity];
    //                     var valueBuffer = getValueBuffer[mainGraphSingleton];
    //                     var instructionsBuffer = getTerrainInstructionBuffer[mainGraphSingleton];
    //
    //                     //todo reimplement hashing
    //                     // hasher.Execute(ref distanceField.DistanceFieldChunkData, chunkPosition, clusterParameters, clusterChild, frameCount);
    //                     //
    //                     // if (!distanceField.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
    //                     // {
    //                     //     return;
    //                     // }
    //
    //                     DistanceFieldResolver.CalculateDistanceFieldForChunk(terrainChunkBuffer, ref distanceField.DistanceFieldChunkData, chunkPosition, instructionsBuffer, distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer, isPlaying, clusterParameters,
    //                         valueBuffer.AsNativeArray().Reinterpret<float>());
    //                 }).WithNativeDisableParallelForRestriction(terrainChunkBuffer).WithReadOnly(getTerrainInstructionBuffer).WithNativeDisableParallelForRestriction(getValueBuffer)
    //                 .WithReadOnly(getClusterParameters).WithBurst().ScheduleParallel(Dependency);
    //         }
    //     }
    //
    //     #endregion
    // }
}