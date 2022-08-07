using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.Systems;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public struct ReadbackHandler
    {
        private int _index;
        [ReadOnly] private BufferFromEntity<CGeometryLayerChild> _getGeometryLayerChild;
        [NativeDisableParallelForRestriction] public BufferFromEntity<PackedDistanceFieldData> GetPackDistanceFieldData;
        [ReadOnly] private ComponentDataFromEntity<CGeometryChunkState> _getChunkState;
        private readonly bool _isPrepass;
        [ReadOnly] private NativeParallelHashMap<Entity, Entity> _prepassSystemMap;
        [NativeDisableParallelForRestriction]public BufferFromEntity<SChunkPrepass.CPrepassContentHash> GetPrepassHashBuffer;

        public ReadbackHandler(SystemBase system)
        {
            _isPrepass = system is SChunkPrepass;

            _prepassSystemMap = system.World.GetExistingSystem<SChunkPrepass>().prepassSystemMap;

            _getGeometryLayerChild = system.GetBufferFromEntity<CGeometryLayerChild>();
            GetPackDistanceFieldData = system.GetBufferFromEntity<PackedDistanceFieldData>();
            GetPrepassHashBuffer = system.GetBufferFromEntity<SChunkPrepass.CPrepassContentHash>();
            _getChunkState = system.GetComponentDataFromEntity<CGeometryChunkState>();

            _index = 0;
        }

        public void SetEntityIndex(int index)
        {
            _index = index;
        }

        public void DoReadback(NativeSlice<PackedDistanceFieldData> targetSlice,
            GeometryInstruction geometryInstruction, NativeSlice<GeometryInstructionHash> hashes)
        {
            if (_isPrepass)
            {
                var sourceLayerPrepass = _prepassSystemMap[geometryInstruction.ReferenceEntity];
                var sourceDistanceFieldBuffer = GetPackDistanceFieldData[sourceLayerPrepass];
                targetSlice.CopyFrom(sourceDistanceFieldBuffer.AsNativeArray());

                
                var sourceHashBuffer = GetPrepassHashBuffer[sourceLayerPrepass];
                
                for (int i = 0; i < hashes.Length; i++)
                {
                    var hash = hashes[i];
                    var cPrepassContentHash = sourceHashBuffer[i];
                    hash.Append(ref cPrepassContentHash);
                    hashes[i] = hash;
                }
            }
            else
            {
                var chunkEntityInOtherLayer =
                    _getGeometryLayerChild[geometryInstruction.ReferenceEntity][_index].ClusterEntity;
                var stateOfOtherChunk = _getChunkState[chunkEntityInOtherLayer];
                if (stateOfOtherChunk.HasContent)
                {
                    var readbackBuffer = GetPackDistanceFieldData[chunkEntityInOtherLayer];
                    targetSlice.CopyFrom(readbackBuffer.AsNativeArray());
                }
                else
                {
                    PackedDistanceFieldData targetValue;
                    if (stateOfOtherChunk.IsFullyInsideGeometry)
                        targetValue = new PackedDistanceFieldData(-Constants.DefaultDOutsideDistance);
                    else
                        targetValue = new PackedDistanceFieldData(Constants.DefaultDOutsideDistance);

                    for (var i = 0; i < targetSlice.Length; i++) targetSlice[i] = targetValue;
                }
            }
        }
    }
}