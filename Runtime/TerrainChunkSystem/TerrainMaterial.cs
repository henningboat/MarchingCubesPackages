using System;
using System.Runtime.InteropServices;

namespace TerrainChunkSystem
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
    }
}