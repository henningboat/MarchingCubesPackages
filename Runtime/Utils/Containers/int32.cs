using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Utils.Containers
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct int32
    {
        [SerializeField] private int4 c0;
        [SerializeField] private int4 c1;
        [SerializeField] private int4 c2;
        [SerializeField] private int4 c3;
        [SerializeField] private int4 c4;
        [SerializeField] private int4 c5;
        [SerializeField] private int4 c6;
        [SerializeField] private int4 c7;

        public unsafe ref int this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 32)
                {
                    throw new ArgumentException("index must be between[0...32]");
                }
#endif
                fixed (int32* array = &this)
                {
                    return ref ((int*) array)[index];
                }
            }
        }

        public void AddOffset(int valueBufferOffset)
        {
            c0 += valueBufferOffset;
            c1 += valueBufferOffset;
            c2 += valueBufferOffset;
            c3 += valueBufferOffset;
        }
    }
}