using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public struct ReadbackHandler
    { 
        private int _index;
        [ReadOnly]private  BufferFromEntity<CGeometryLayerChild> _getGeometryLayerChild;
        [NativeDisableParallelForRestriction]public  BufferFromEntity<PackedDistanceFieldData> GetPackDistanceFieldData;
        [ReadOnly]private  ComponentDataFromEntity<CGeometryChunkState> _getChunkState; 
        private bool _isPrepass;

        public ReadbackHandler(SystemBase system)
        {
            _getGeometryLayerChild = system.GetBufferFromEntity<CGeometryLayerChild>();
            GetPackDistanceFieldData = system.GetBufferFromEntity<PackedDistanceFieldData>();
            _getChunkState = system.GetComponentDataFromEntity<CGeometryChunkState>();

            _isPrepass = true;
            _index = 0;
        }

        public void SetEntityIndex(int index)
        {
            _isPrepass = false;
            _index = index;
        }

        public void DoReadback(NativeSlice<PackedDistanceFieldData> targetSlice,
            GeometryInstruction geometryInstruction)
        {
            if (_isPrepass)
            {
                //todo add this
                for (var i = 0; i < targetSlice.Length; i++) targetSlice[i] = new PackedDistanceFieldData(0);
            }
            else
            {
                var chunkEntityInOtherLayer =
                    _getGeometryLayerChild[geometryInstruction.ReferenceEntity][_index].ClusterEntity;
                var readbackBuffer = GetPackDistanceFieldData[chunkEntityInOtherLayer];
                targetSlice.CopyFrom(readbackBuffer.AsNativeArray());
            }
        }
    }
}