using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.BinaryAssets
{
    public struct SDF2DHeader : IBinaryAssetHeader
    {
        public int DataIndex { get; set; }
        public int DataLength { get; set; }
        public int AssetInstanceID { get; set; }
        
        private int2 _size;

        public float Sample(float2 uv, NativeSlice<float> data)
        {
            uv = math.clamp(uv, 0, _size - 2);
            var flooredUV = (int2) math.floor(uv);

            var c00 = SamplePixel(flooredUV, data);
            var c10 = SamplePixel(flooredUV + new int2(1, 0), data);
            var c01 = SamplePixel(flooredUV + new int2(0, 1), data);
            var c11 = SamplePixel(flooredUV + new int2(1, 1), data);

            var subPixelPosition = uv % 1;
            var row0 = math.lerp(c00, c10, subPixelPosition.x);
            var row1 = math.lerp(c01, c11, subPixelPosition.x);

            var interpolatedValue = math.lerp(row0, row1, subPixelPosition.y);
            return interpolatedValue;
        }


        public float SamplePixel(int2 pixel, NativeSlice<float> data)
        {
            var index = DistanceFieldGeneration.Utils.PositionToIndex(pixel, _size);
            return data[index];
        }

        public static void CreateSDF(BinaryDataStorage binaryDataStorage, Texture2D sdfTexture)
        {
            var size = new int2(sdfTexture.width, sdfTexture.height);

            var header = new SDF2DHeader()
                {_size = size, DataLength = size.x * size.y, AssetInstanceID = sdfTexture.GetInstanceID()};
            
            binaryDataStorage.CreateSDF2DData(header, out NativeSlice<float> data);

            var textureData = sdfTexture.GetPixels();

            for (var i = 0; i < size.x * size.y; i++) data[i] = (0.5f - textureData[i].r);
        }
    }
}