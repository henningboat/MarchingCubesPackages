using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Utils.Containers
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct float32
    {
        [SerializeField] private float4 c0;
        [SerializeField] private float4 c1;
        [SerializeField] private float4 c2;
        [SerializeField] private float4 c3;
        [SerializeField] private float4 c4;
        [SerializeField] private float4 c5;
        [SerializeField] private float4 c6;
        [SerializeField] private float4 c7;

        public unsafe ref float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 32) throw new ArgumentException("index must be between[0...32]");
#endif
                fixed (float32* array = &this)
                {
                    return ref ((float*) array)[index];
                }
            }
        }
    }
}