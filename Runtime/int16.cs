using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[Serializable]
public struct int16
{
    [SerializeField] private int4 c0;
    [SerializeField] private int4 c1;
    [SerializeField] private int4 c2;
    [SerializeField] private int4 c3;

    public unsafe ref int this[int index]
    {
        get
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint) index >= 16)
            {
                throw new ArgumentException("index must be between[0...15]");
            }
#endif
            fixed (int16* array = &this)
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