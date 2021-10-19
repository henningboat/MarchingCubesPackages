using Rendering;
using TerrainChunkSystem;
using Unity.Collections;

namespace NonECSImplementation
{
    public struct GeometryCluster
    {
        private NativeSlice<PackedDistanceFieldData> _distanceField;
        private NativeSlice<CClusterParameters> _clusterParameters;
        private NativeSlice<GeometryChunkParameters> _chunkParametersSlice;

        public GeometryCluster(NativeSlice<PackedDistanceFieldData> distanceField, NativeSlice<CClusterParameters> clusterParameters, NativeSlice<GeometryChunkParameters> chunkParametersSlice)
        {
            _distanceField = distanceField;
            _clusterParameters = clusterParameters;
            _chunkParametersSlice = chunkParametersSlice;
        }

        public CClusterParameters Parameters => _clusterParameters[0];

        public GeometryChunk GetChunk(int chunkIndex)
        {
            var distanceField = _distanceField.Slice(chunkIndex * GeometryFieldData.chunkVolume / 4, GeometryFieldData.chunkVolume / 4);
            var chunkParameters = _chunkParametersSlice.Slice(chunkIndex, 1);
           
            return new GeometryChunk(distanceField, chunkParameters);
        }
    }
}