using System;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.BinaryAssets
{
    public struct AssetDataHolder<THeader, TData>:IDisposable where THeader : unmanaged, IBinaryAssetHeader where TData : unmanaged
    {
        public NativeList<THeader> Headers;
        public NativeList<TData> Data;
        
        public AssetDataHolder(Allocator allocator)
        {
            Headers = new NativeList<THeader>(allocator);
            Data = new NativeList<TData>(allocator);
        }

        public void Dispose()
        {
            Headers.Dispose();
            Data.Dispose();
        }


        public void GetAsset(int assetIndex, out THeader header, out NativeSlice<TData> data)
        {
            header = Headers[assetIndex];
            data = Data.AsArray().Slice(header.DataIndex, header.DataLength);
        }

        public void AddHeaderAndGetDataSlice(THeader header, out NativeSlice<TData> nativeSlice)
        {
            header.DataIndex = Data.Length;
            Headers.Add(header);
            Data.Length += header.DataLength;
            nativeSlice=Data.AsArray().Slice(header.DataIndex, header.DataLength);
        }

        public int GetEntryWithInstanceID(int instanceID)
        {
            for (int i = 0; i < Headers.Length; i++)
            {
                var binaryAssetHeader = Headers[i];
                if (binaryAssetHeader.AssetInstanceID == instanceID)
                {
                    return  i;
                }
            }

            throw new ArgumentException();
        }
        
        public bool ContainsObject(int instanceID)
        {
            for (int i = 0; i < Headers.Length; i++)
            {
                var binaryAssetHeader = Headers[i];
                if (binaryAssetHeader.AssetInstanceID == instanceID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}