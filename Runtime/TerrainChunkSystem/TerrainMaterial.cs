using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace henningboat.CubeMarching.TerrainChunkSystem
{
    /// <summary>
    ///     Always make sure to keep the content of this struct in sync with TerrainMaterial.hlsl
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct TerrainMaterial
    {
        public byte R;
        public byte G;
        public byte B;
        public byte MaterialID;

        public static TerrainMaterial GetDefaultMaterial()
        {
            return new() {R = byte.MaxValue, G = Byte.MaxValue, B = Byte.MaxValue, MaterialID = Byte.MaxValue};
        }

        public float3 GetColor => new float3(R, G, B) / 255f;

        public void SetColor(float3 float3Value)
        {
            float3Value *= 255;
            R = (byte) float3Value.x;
            G = (byte) float3Value.y;
            B = (byte) float3Value.z;
        }
    }
}