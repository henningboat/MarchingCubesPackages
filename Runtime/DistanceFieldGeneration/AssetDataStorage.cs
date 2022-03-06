using System;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public void EnsureAssetInStorage(Object assetDependency, out int geometryInstructionAssetIndexInGlobalBuffer)
        {
            var newInstanceID = assetDependency.GetInstanceID();
            var needsToBeAdded = true;
            foreach (var existingAsset in Assets)
            {
                if (existingAsset.InstanceID == newInstanceID)
                {
                    needsToBeAdded = false;
                    break;
                }
            }

            if (needsToBeAdded)
            {
                switch (assetDependency)
                {
                    case Texture2D texture2D:
                        SDF2DData.Create(texture2D, this);
                        break;
                    default:
                        throw new NotImplementedException(
                            $"Assets of type {assetDependency.GetType()} can't be added to the Asset Data Storage");
                }
            }

            geometryInstructionAssetIndexInGlobalBuffer =
                Assets.ToArray().First(entry => entry.InstanceID == newInstanceID).ByteOffsetInDataBuffer;
        }
    }
}