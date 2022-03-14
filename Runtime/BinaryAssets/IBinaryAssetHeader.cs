namespace henningboat.CubeMarching.Runtime.BinaryAssets
{
    public interface IBinaryAssetHeader
    {
        public int DataIndex { get; set; }
        public int DataLength { get; set; }
        public int AssetInstanceID { get; set; }
    }
}