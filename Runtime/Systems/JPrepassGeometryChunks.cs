using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SSetupGeometryLayers))]
    public partial class SChunkPrepass : SGeometrySystem
    {
        private Dictionary<GeometryLayerAssetsReference, NativeList<Entity>> _dirtyChunksPerLayer;

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype(typeof(CGeometryGraphChunkPrepassTag), typeof(PackedDistanceFieldData),
                typeof(CPrepassPackedWorldPosition));
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return true;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _dirtyChunksPerLayer = new Dictionary<GeometryLayerAssetsReference, NativeList<Entity>>();
        }

        protected override void OnStopRunning()
        {
            foreach (var nativeList in _dirtyChunksPerLayer.Values) nativeList.Dispose();

            _dirtyChunksPerLayer = null;
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var singleton =
                _setupLayer.GetGeometryLayerSingleton<CGeometryGraphChunkPrepassTag>(geometryLayerReference);

            var instructionsFromEntity = GetBufferFromEntity<GeometryInstruction>(true);
            var prepassDistanceField =
                EntityManager.GetBuffer<PackedDistanceFieldData>(singleton, true).AsNativeArray();

            var job = new JUpdatePrepassDistanceField
            {
                GetInstructionsFromEntity = instructionsFromEntity,
                GetPackedDistanceFieldBufferFromEntity = GetBufferFromEntity<PackedDistanceFieldData>(),
                MainEntity = singleton,
                GeometryLayerReference = EntityManager.GetChunkComponentData<CGeometryLayerReference>(singleton),
                PositionWS = EntityManager.GetBuffer<CPrepassPackedWorldPosition>(singleton),
                ResultBuffer = prepassDistanceField
            };
            Dependency = job.Schedule(Dependency);

            var dirtyList = _dirtyChunksPerLayer[geometryLayerReference];
            dirtyList.Clear();
            var dirtyListWriter = dirtyList.AsParallelWriter();


            Dependency = Entities.ForEach(
                    (Entity entity, ref CGeometryChunkState chunkState, in CGeometryChunk chunk) =>
                    {
                        var index = chunk.IndexInIndexMap;
                        var a = prepassDistanceField[index * 2].SurfaceDistance;
                        var b = prepassDistanceField[index * 2 + 1].SurfaceDistance;

                        const float maxDistance = Constants.chunkLength;

                        var aInside = math.abs(a.PackedValues) < maxDistance;
                        var bInside = math.abs(b.PackedValues) < maxDistance;

                        var hasContent = math.any(aInside | bInside);
                        
                        if (hasContent || chunkState.HasContent)
                            if (hasContent)
                                dirtyListWriter.AddNoResize(entity);
                        
                        chunkState.HasContent = hasContent;
                    })
                .WithReadOnly(prepassDistanceField).WithBurst().ScheduleParallel(Dependency);
            
            Dependency.Complete();
            Debug.Log(dirtyList.Length);
        }

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity,
            CGeometryFieldSettings settings)
        {
            var chunkCount = settings.ClusterCounts.Volume();
            var distanceFieldBuffer = EntityManager.GetBuffer<PackedDistanceFieldData>(entity);
            var positionBuffer = EntityManager.GetBuffer<CPrepassPackedWorldPosition>(entity);

            //we want 8 values per chunk
            distanceFieldBuffer.ResizeUninitialized(chunkCount * 2);
            distanceFieldBuffer.Length = chunkCount * 2;
            positionBuffer.ResizeUninitialized(chunkCount * 2);
            positionBuffer.Length = chunkCount * 2;

            var geometryLayerAssetsReference = new GeometryLayerAssetsReference(layer);

            var query = GetEntityQuery(typeof(CGeometryChunk), typeof(GeometryLayerAssetsReference));
            query.AddSharedComponentFilter(geometryLayerAssetsReference);
            var chunksOfLayer = query.ToComponentDataArray<CGeometryChunk>(Allocator.Temp);

            var offsetsA = new PackedFloat3(
                new float3(0.25f, 0.25f, 0.25f),
                new float3(0.75f, 0.25f, 0.25f),
                new float3(0.25f, 0.75f, 0.25f),
                new float3(0.75f, 0.75f, 0.25f)) * Constants.chunkLength;

            var offsetsB = new PackedFloat3(
                new float3(0.25f, 0.25f, 0.75f),
                new float3(0.75f, 0.25f, 0.75f),
                new float3(0.25f, 0.75f, 0.75f),
                new float3(0.75f, 0.75f, 0.75f)) * Constants.chunkLength;

            for (var chunkIndex = 0; chunkIndex < chunksOfLayer.Length; chunkIndex++)
            {
                var chunk = chunksOfLayer[chunkIndex];

                positionBuffer[chunkIndex * 2 + 0] = new CPrepassPackedWorldPosition
                    {Value = chunk.PositionWS + offsetsA};
                positionBuffer[chunkIndex * 2 + 1] = new CPrepassPackedWorldPosition
                    {Value = chunk.PositionWS + offsetsB};
            }

            var nativeList = new NativeList<Entity>(Allocator.Persistent);
            nativeList.Length = settings.ClusterCounts.Volume();
            ;
            _dirtyChunksPerLayer[geometryLayerAssetsReference] = nativeList;
        }

        public struct CPrepassPackedWorldPosition : IBufferElementData
        {
            public PackedFloat3 Value;
        }

        public struct CGeometryGraphChunkPrepassTag : IComponentData
        {
        }

        private struct JUpdatePrepassDistanceField : IJob
        {
            [ReadOnly] public BufferFromEntity<GeometryInstruction> GetInstructionsFromEntity;

            public DynamicBuffer<CPrepassPackedWorldPosition> PositionWS;

            [NativeDisableParallelForRestriction] [NativeDisableContainerSafetyRestriction]
            public BufferFromEntity<PackedDistanceFieldData> GetPackedDistanceFieldBufferFromEntity;

            public Entity MainEntity;
            public CGeometryLayerReference GeometryLayerReference;
            public NativeArray<PackedDistanceFieldData> ResultBuffer;

            public void Execute()
            {
                var layerEntity = GeometryLayerReference.LayerEntity;

                var instructions = GetInstructionsFromEntity[layerEntity];

                var iterator = new GeometryInstructionIterator(default, instructions, default, default,
                    GetPackedDistanceFieldBufferFromEntity, default,
                    PositionWS.Reinterpret<PackedFloat3>().AsNativeArray());

                iterator.CalculateAllTerrainData();

                ResultBuffer.Slice(0, ResultBuffer.Length)
                    .CopyFrom(iterator._terrainDataBuffer.Slice(0, ResultBuffer.Length));
                iterator.Dispose();
            }
        }
    }
}