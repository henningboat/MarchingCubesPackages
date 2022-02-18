namespace henningboat.CubeMarching.Runtime
{
    public static class Constants
    {
        public const int chunksPerCluster = 8 * 8 * 8;
        public const int chunkVolume = 8 * 8 * 8;
        public const int subChunkVolume = subChunkLength * subChunkLength * subChunkLength;
        public const int chunkLength = 8;
        public const int subChunkLength = 4;
        public const int subChunksPerChunk = 8;
        public const int subChunksPerCluster = chunksPerCluster * subChunksPerChunk;
        public const int clusterLength = chunkLength * 8;
        public const int clusterVolume = clusterLength * clusterLength * clusterLength;
        public const int chunkLengthPerCluster = 8;
        public const int PackedCapacity = 4;
        public const int maxVertsPerCluster = 4 * 4 * 4 * 5 * 3;
    }
}