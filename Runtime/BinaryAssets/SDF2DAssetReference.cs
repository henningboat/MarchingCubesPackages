namespace henningboat.CubeMarching.Runtime.BinaryAssets
{
    public struct SDF2DAssetReference : IBinaryAssetReference
    {
        private float _assetIndex;

        public int AssetIndex
        {
            get => (int)_assetIndex;
        }
    }
}