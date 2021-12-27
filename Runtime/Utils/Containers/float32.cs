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


        public static float32 FromFloat(float constant)
        {
            var value = new float32
            {
                [0] = constant
            };
            return value;
        }

        public static float32 FromFloat3(float3 constant)
        {
            var value = new float32
            {
                [0] = constant.x,
                [1] = constant.y,
                [2] = constant.z
            };
            return value;
        }

        public static float32 FromFloat4x4(float4x4 constant)
        {
            return FromMatrix(constant);
        }

        public static float32 FromMatrix(Matrix4x4 constant)
        {
            var value = new float32();

            for (var i = 0; i < 16; i++) value[i] = constant[i];

            return value;
        }


        public static float32 GetFloat32ForObject(object value)
        {
            if (value is float floatValue) return FromFloat(floatValue);
            if (value is float3 float3Value) return FromFloat3(float3Value);
            if (value is float4x4 float4x4Value) return FromFloat4x4(float4x4Value);

            throw new ArgumentOutOfRangeException();
        }

        public static float32 GetFloat32FromFloatArray(float[] floatArray)
        {
            float32 result = default;
            for (var i = 0; i < floatArray.Length; i++) result[i] = floatArray[i];

            return result;
        }
    }
}