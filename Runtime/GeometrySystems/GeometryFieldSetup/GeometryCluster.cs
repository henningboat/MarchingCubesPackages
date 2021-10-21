using henningboat.CubeMarching.TerrainChunkSystem;
using Unity.Collections;

namespace henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup
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

        public CClusterParameters Parameters
        {
            get => _clusterParameters[0];
            set => _clusterParameters[0] = value;
        }

        public GeometryChunk GetChunk(int chunkIndex)
        {
            var distanceField = _distanceField.Slice(chunkIndex * GeometryFieldData.chunkVolume / 4, GeometryFieldData.chunkVolume / 4);
            var chunkParameters = _chunkParametersSlice.Slice(chunkIndex, 1);
           
            return new GeometryChunk(distanceField, chunkParameters);
        }
    }
}