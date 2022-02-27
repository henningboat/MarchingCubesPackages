using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    public unsafe struct AssetDataStorage : IDisposable
    {
        private NativeList<byte> DataBuffer;
        private NativeList<AssetEntry> Assets;

        public AssetDataStorage(Allocator allocator)
        {
            DataBuffer = new NativeList<byte>(allocator);
            Assets = new NativeList<AssetEntry>(allocator);
        }

        public T* GetData<T>(int addressInsideDataBuffer) where T : unmanaged
        {
            return (T*) DataBuffer.GetUnsafeReadOnlyPtr() + (ulong) addressInsideDataBuffer;
        }

        public T* CreateAsset<T>(int instanceID, int dataBufferSize) where T : unmanaged
        {
            var assetEntry = new AssetEntry(instanceID, DataBuffer.Length);
            Assets.Add(assetEntry);

            var totalSize = sizeof(T) + dataBufferSize;
            DataBuffer.Capacity += totalSize;
            DataBuffer.Length += totalSize;

            return (T*) DataBuffer.GetUnsafePtr() + (ulong) assetEntry.ByteOffsetInDataBuffer;
        }

        public void Dispose()
        {
            if (DataBuffer.IsCreated) DataBuffer.Dispose();
            if (Assets.IsCreated) Assets.Dispose();
        }

        private readonly struct AssetEntry
        {
            public readonly int InstanceID;
            public readonly int ByteOffsetInDataBuffer;

            public AssetEntry(int instanceID, int byteOffsetInDataBuffer)
            {
                InstanceID = instanceID;
                ByteOffsetInDataBuffer = byteOffsetInDataBuffer;
            }
        }
    }
}