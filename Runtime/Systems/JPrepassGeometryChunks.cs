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
        /// <summary>
        /// Maps a layer entity to it's prepass entity, used by ReadbackHandler
        /// </summary>
        private NativeParallelHashMap<Entity, Entity> _prepassSystemMap;

        /// <summary>
        /// Maps a layer entity to it's prepass entity, used by ReadbackHandler
        /// </summary>
        public NativeParallelHashMap<Entity, Entity> prepassSystemMap => _prepassSystemMap;

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
            //todo placeholder capacity
            _prepassSystemMap = new NativeParallelHashMap<Entity, Entity>(10,Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var nativeList in _dirtyChunksPerLayer.Values) nativeList.Dispose();
            foreach (var nativeList in _chunksWithContent.Values) nativeList.Dispose();
        
            _prepassSystemMap.Dispose();

            _dirtyChunksPerLayer = null;
            _chunksWithContent = null;
            
            base.OnDestroy();
        }

        public override void UpdateInternal(GeometryLayerAssetsReference geometryLayerReference)
        {
            var singleton =
                _setupLayer.GetGeometryLayerSingleton<CGeometryGraphChunkPrepassTag>(geometryLayerReference);

            var mainEntity = EntityManager.GetChunkComponentData<CGeometryLayerReference>(singleton);
            var buffer = GetBuffer<GeometryInstruction>(mainEntity.LayerEntity,true).AsNativeArray();

            var job = new JUpdatePrepassDistanceField
            {
                Instructions = buffer,
                PositionWS = EntityManager.GetBuffer<CPrepassPackedWorldPosition>(singleton),
                ReadbackHandler = new ReadbackHandler(this),
                MainEntity = singleton,
            };
            Dependency=job.Schedule(Dependency);
            // //todo placeholder
             Dependency.Complete();

            var dirtyList = _dirtyChunksPerLayer[geometryLayerReference];
            dirtyList.Clear();
            var dirtyListWriter = dirtyList.AsParallelWriter();

            var contentList = _chunksWithContent[geometryLayerReference];
            contentList.Clear();
             var contentListWriter = contentList.AsParallelWriter();
            
             var prepassDistanceField =
                 EntityManager.GetBuffer<PackedDistanceFieldData>(singleton, true).AsNativeArray();

             var prepassHashData =
                 EntityManager.GetBuffer<CPrepassContentHash>(singleton, true).AsNativeArray();
             
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
            
                         chunkState.IsFullyInsideGeometry = SimdMath.any(a.PackedValues < new float8(0f));
                         
                         if (chunkState.IsDirty)
                         {
                             dirtyListWriter.AddNoResize(entity);
                         }
                     })
                 .WithReadOnly(prepassDistanceField).WithReadOnly(prepassHashData).WithBurst().Schedule(Dependency);
            
            
            Dependency.Complete();
        } 

        protected override void InitializeLayerHandlerEntity(GeometryLayerAsset layer, Entity entity, Entity layerEntity,
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

            _prepassSystemMap.Add(layerEntity, entity);
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
            [ReadOnly]public NativeArray<GeometryInstruction> Instructions;

            public DynamicBuffer<CPrepassPackedWorldPosition> PositionWS;

             public ReadbackHandler ReadbackHandler;

            public Entity MainEntity;

            public void Execute()
            {
                var iterator = new GeometryInstructionIterator(default, Instructions, default,
                    PositionWS.Reinterpret<PackedFloat3>().AsNativeArray(), true,ReadbackHandler);

                iterator.ProcessAllInstructions();

                var resultBuffer = ReadbackHandler.GetPackDistanceFieldData[MainEntity].AsNativeArray();
                var resultHashes = ReadbackHandler.GetPrepassHashBuffer[MainEntity].AsNativeArray();
                
                resultBuffer.Slice(0, resultBuffer.Length)
                    .CopyFrom(iterator._terrainDataBuffer.Slice(0, resultBuffer.Length));
                resultHashes.Reinterpret<GeometryInstructionHash>().Slice(0, iterator._contentHashBuffer.Length)
                    .CopyFrom(iterator._contentHashBuffer.Slice(0, iterator._contentHashBuffer.Length));
                
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