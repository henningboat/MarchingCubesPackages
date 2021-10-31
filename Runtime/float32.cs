using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace henningboat.CubeMarching
{
    [StructLayout(LayoutKind.Sequential)]
    public struct float32
    {
        private float4 c0;
        private float4 c1;
        private float4 c2;
        private float4 c3;
        private float4 c4;
        private float4 c5;
        private float4 c6;
        private float4 c7;

        public unsafe ref float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 32)
                {
                    throw new System.ArgumentException("index must be between[0...32]");
                }
#endif
                fixed (float32* array = &this)
                {
                    return ref ((float*) array)[index];
                }
            }
        }
    }
}