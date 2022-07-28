using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using SIMDMath;
using Unity.Burst;
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
        private Dictionary<GeometryLayerAssetsReference, NativeList<Entity>> _chunksWithContent;

        protected override EntityArchetype GetArchetype()
        {
            return EntityManager.CreateArchetype(typeof(CGeometryGraphChunkPrepassTag), typeof(PackedDistanceFieldData),
                typeof(CPrepassPackedWorldPosition), typeof(CPrepassContentHash));
        }

        protected override bool RunSystemForLayer(GeometryLayerAsset layer)
        {
            return true;
        }

        protected override void OnCreate()
        {
            _dirtyChunksPerLayer = new Dictionary<GeometryLayerAssetsReference, NativeList<Entity>>();
            _chunksWithContent = new Dictionary<GeometryLayerAssetsReference, NativeList<Entity>>();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var nativeList in _dirtyChunksPerLayer.Values) nativeList.Dispose();
            foreach (var nativeList in _chunksWithContent.Values) nativeList.Dispose();

            _dirtyChunksPerLayer = null;
            _chunksWithContent = null;
            
            base.OnDestroy();
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var singleton =
                _setupLayer.GetGeometryLayerSingleton<CGeometryGraphChunkPrepassTag>(geometryLayerReference);

            var instructionsFromEntity = GetBufferFromEntity<GeometryInstruction>(true);
            var prepassDistanceField =
                EntityManager.GetBuffer<PackedDistanceFieldData>(singleton, true).AsNativeArray();
            var prepassHashData =
                EntityManager.GetBuffer<CPrepassContentHash>(singleton, true).AsNativeArray();

            var job = new JUpdatePrepassDistanceField
            {
                GetInstructionsFromEntity = instructionsFromEntity,
                GetPackedDistanceFieldBufferFromEntity = GetBufferFromEntity<PackedDistanceFieldData>(),
                MainEntity = singleton,
                GeometryLayerReference = EntityManager.GetChunkComponentData<CGeometryLayerReference>(singleton),
                PositionWS = EntityManager.GetBuffer<CPrepassPackedWorldPosition>(singleton),
                ResultBuffer = prepassDistanceField,
                ContentHashPerChunk = prepassHashData.Reinterpret<GeometryInstructionHash>(),
            };
            Dependency = job.Schedule(Dependency);

            var dirtyList = _dirtyChunksPerLayer[geometryLayerReference];
            dirtyList.Clear();
            var dirtyListWriter = dirtyList.AsParallelWriter();

            var contentList = _chunksWithContent[geometryLayerReference];
            contentList.Clear();
            var contentListWriter = contentList.AsParallelWriter();

            Dependency = Entities.WithSharedComponentFilter(geometryLayerReference).ForEach(
                    (Entity entity, ref CGeometryChunkState chunkState, in CGeometryChunk chunk) =>
                    {
                        var index = chunk.IndexInIndexMap;
                        var a = prepassDistanceField[index].SurfaceDistance;

                        const float maxDistance = Constants.chunkLength / 2.0f;

                        var aInside = SimdMath.abs(a) < maxDistance;

                        var hasContent = SimdMath.any(aInside);

                        if (hasContent || chunkState.HasContent)
                        {
                           contentListWriter.AddNoResize(entity);
                        }

                        chunkState.HasContent = hasContent;
                        var newContentHash = prepassHashData[index].Value;
                        chunkState.IsDirty = !newContentHash.Equals(chunkState.ContentHash);
                        chunkState.ContentHash = newContentHash;

                        if (chunkState.IsDirty)
                        {
                            dirtyListWriter.AddNoResize(entity);
                        }
                    })
                .WithReadOnly(prepassDistanceField).WithReadOnly(prepassHashData).WithBurst().ScheduleParallel(Dependency);
        } 

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity,
            CGeometryFieldSettings settings)
        {
            var chunkCount = settings.ClusterCounts.Volume();
            var distanceFieldBuffer = EntityManager.GetBuffer<PackedDistanceFieldData>(entity);
            var positionBuffer = EntityManager.GetBuffer<CPrepassPackedWorldPosition>(entity);
            var contentHash = EntityManager.GetBuffer<CPrepassContentHash>(entity);

            //we want 8 values per chunk
            distanceFieldBuffer.ResizeUninitialized(chunkCount);
            distanceFieldBuffer.Length = chunkCount;
            positionBuffer.ResizeUninitialized(chunkCount);
            positionBuffer.Length = chunkCount;

            contentHash.Length = chunkCount;
            
            var geometryLayerAssetsReference = new GeometryLayerAssetsReference(layer);

            var query = GetEntityQuery(typeof(CGeometryChunk), typeof(GeometryLayerAssetsReference));
            query.SetSharedComponentFilter(geometryLayerAssetsReference);
            var chunksOfLayer = query.ToComponentDataArray<CGeometryChunk>(Allocator.Temp);

            var offsetsA = new PackedFloat3(
                new float3(0.25f, 0.25f, 0.25f),
                new float3(0.75f, 0.25f, 0.25f),
                new float3(0.25f, 0.75f, 0.25f),
                new float3(0.75f, 0.75f, 0.25f),
                new float3(0.25f, 0.25f, 0.75f),
                new float3(0.75f, 0.25f, 0.75f),
                new float3(0.25f, 0.75f, 0.75f),
                new float3(0.75f, 0.75f, 0.75f)) * Constants.chunkLength;

            for (var chunkIndex = 0; chunkIndex < chunksOfLayer.Length; chunkIndex++)
            {
                var chunk = chunksOfLayer[chunkIndex];

                positionBuffer[chunkIndex] = new CPrepassPackedWorldPosition
                    {Value = chunk.PositionWS + offsetsA};
            }

            var dirtyChunks = new NativeList<Entity>(Allocator.Persistent);
            dirtyChunks.Length = settings.ClusterCounts.Volume();
            _dirtyChunksPerLayer[geometryLayerAssetsReference] = dirtyChunks; 
            
            var contentChunks = new NativeList<Entity>(Allocator.Persistent);
            contentChunks.Length = settings.ClusterCounts.Volume();
            _chunksWithContent[geometryLayerAssetsReference] = contentChunks; 

        }

        public struct CPrepassPackedWorldPosition : IBufferElementData
        {
            public PackedFloat3 Value;
        }

        public struct CPrepassContentHash : IBufferElementData
        {
            public GeometryInstructionHash Value;
        }

        public struct CGeometryGraphChunkPrepassTag : IComponentData
        {
        }

        [BurstCompile]
        private struct JUpdatePrepassDistanceField : IJob
        {
            [ReadOnly] public BufferFromEntity<GeometryInstruction> GetInstructionsFromEntity;

            public DynamicBuffer<CPrepassPackedWorldPosition> PositionWS;

            [NativeDisableParallelForRestriction] [NativeDisableContainerSafetyRestriction]
            public BufferFromEntity<PackedDistanceFieldData> GetPackedDistanceFieldBufferFromEntity;

            public Entity MainEntity;
            public CGeometryLayerReference GeometryLayerReference;
            public NativeArray<PackedDistanceFieldData> ResultBuffer;
            public NativeArray<GeometryInstructionHash> ContentHashPerChunk;

            public void Execute()
            {
                var layerEntity = GeometryLayerReference.LayerEntity;

                var instructions = GetInstructionsFromEntity[layerEntity];

                var iterator = new GeometryInstructionIterator(default, instructions, default, default,
                    GetPackedDistanceFieldBufferFromEntity, default,
                    PositionWS.Reinterpret<PackedFloat3>().AsNativeArray(), true);

                iterator.ProcessAllInstructions();

                ResultBuffer.Slice(0, ResultBuffer.Length)
                    .CopyFrom(iterator._terrainDataBuffer.Slice(0, ResultBuffer.Length));
                ContentHashPerChunk.Slice(0, ContentHashPerChunk.Length)
                    .CopyFrom(iterator._contentHashBuffer.Slice(0, ContentHashPerChunk.Length));
                
                iterator.Dispose();
            }
        }

        public NativeList<Entity> GetDirtyChunks(GeometryLayerAssetsReference geometryLayerReference)
        {
            return _dirtyChunksPerLayer[geometryLayerReference];
        }
        
        public NativeList<Entity> GetChunksToDraw(GeometryLayerAssetsReference geometryLayerReference)
        {
            return _chunksWithContent[geometryLayerReference];
        }
    }
}