﻿using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using Unity.Collections;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup
{
    public struct GeometryChunk
    {
        private NativeSlice<PackedDistanceFieldData> _distanceField;
        private NativeSlice<GeometryChunkParameters> _chunkParametersSlice;

        public GeometryChunk(NativeSlice<PackedDistanceFieldData> distanceField,
            NativeSlice<GeometryChunkParameters> chunkParametersSlice)
        {
            _distanceField = distanceField;
            _chunkParametersSlice = chunkParametersSlice;
        }

        public GeometryChunkParameters Parameters
        {
            get => _chunkParametersSlice[0];
            set => _chunkParametersSlice[0] = value;
        }

        public PackedDistanceFieldData this[int i]
        {
            get => _distanceField[i];
            set => _distanceField[i] = value;
        }
    }
}