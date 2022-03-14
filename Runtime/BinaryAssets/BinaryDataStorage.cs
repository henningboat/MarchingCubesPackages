using System;
using System.Globalization;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.BinaryAssets
{
    public struct BinaryDataStorage:IDisposable
    {
        private AssetDataHolder<SDF2DHeader, float> _sdf2DData;
        public BinaryDataStorage(Allocator allocator)
        {
            _sdf2DData = new AssetDataHolder<SDF2DHeader, float>(allocator);
        }

        public void GetBinaryAsset(int binaryAssetIndex, out SDF2DHeader header, out NativeSlice<float> data)
        {
            _sdf2DData.GetAsset(binaryAssetIndex, out header, out data);
        }

        public void Dispose()
        {
            if (_sdf2DData.Data.IsCreated)
            {
                _sdf2DData.Dispose();
            }
        }

        public void CreateSDF2DData(SDF2DHeader header, out NativeSlice<float> nativeSlice)
        {
            _sdf2DData.AddHeaderAndGetDataSlice(header, out nativeSlice);
        }

        public int RegisterAssetAndGetIndex(Object overwriteObjectValue)
        {
            switch (overwriteObjectValue)
            {
                case Texture2D texture2D:
                    if (!_sdf2DData.ContainsObject(overwriteObjectValue.GetInstanceID()))
                    {
                        SDF2DHeader.CreateSDF(this, texture2D);
                    }

                    return _sdf2DData.GetEntryWithInstanceID(overwriteObjectValue.GetInstanceID());
                default:
                    throw new CultureNotFoundException();
            }
        }
    }
}