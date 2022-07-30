using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct float8
    {
        public float4 a;
        public float4 b;

        public float8(float4 a, float4 b)
        {
            this.a = a;
            this.b = b;
        }

        public float8(float f, float f1, float f2, float f3, float f4, float f5, float f6, float f7)
        {
            a = new float4(f, f1, f2, f3);
            b = new float4(f4, f5, f6, f7);
        }

        public float8(float value)
        {
            a = value;
            b = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator +(float8 lhs, float8 rhs)
        {
            return new float8(lhs.a + rhs.a, lhs.b + rhs.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator +(float8 lhs, float rhs)
        {
            return new float8(lhs.a + rhs, lhs.b + rhs);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator -(float8 lhs, float8 rhs)
        {
            return new float8(lhs.a - rhs.a, lhs.b - rhs.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator -(float8 lhs, float rhs)
        {
            return new float8(lhs.a - rhs, lhs.b - rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator -(float8 a)
        {
            return new float8(-a.a, -a.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator *(float8 lhs, float8 rhs)
        {
            return new float8(lhs.a * rhs.a, lhs.b * rhs.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator /(float8 lhs, float8 rhs)
        {
            return new float8(lhs.a / rhs.a, lhs.b / rhs.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float8 operator %(float8 lhs, float8 rhs)
        {
            return new float8(lhs.a % rhs.a, lhs.b % rhs.b);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator ==(float8 a, float8 b)
        {
            return new bool8(a.a == b.a, a.b == b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator !=(float8 a, float8 b)
        {
            return new bool8(a.a != b.a, a.b != b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float8(int v)
        {
            return new float8(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator <(float8 lhs, float8 rhs)
        {
            return new bool8(lhs.a < rhs.a, lhs.b < rhs.b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator >(float8 lhs, float8 rhs)
        {
            return new bool8(lhs.a > rhs.a, lhs.b < rhs.b);
        }

        /// <summary>Returns the float element at a specified index.</summary>
        unsafe public float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 8)
                    throw new System.ArgumentException("index must be between[0...8]");
#endif
                fixed (float8* array = &this)
                {
                    return ((float*) array)[index];
                }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 8)
                    throw new System.ArgumentException("index must be between[0...8]");
#endif
                fixed (float* array = &a.x)
                {
                    array[index] = value;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct bool8
    {
        public bool4 a;
        public bool4 b;

        public bool8(bool4 a, bool4 b)
        {
            this.a = a;
            this.b = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator ==(bool8 a, bool8 b)
        {
            return new bool8(a.a == b.a, a.b == b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator !=(bool8 a, bool8 b)
        {
            return new bool8(a.a != b.a, a.b != b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator &(bool8 a, bool8 b)
        {
            return new bool8(a.a & b.a, a.b & b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator |(bool8 a, bool8 b)
        {
            return new bool8(a.a | b.a, a.b | b.b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator !(bool8 val)
        {
            return new bool8(!val.a, !val.b);
        }

        unsafe public bool this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 8)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (bool8* array = &this)
                {
                    return ((bool*) array)[index];
                }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 8)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (bool* array = &a.x)
                {
                    array[index] = value;
                }
            }
        }
    }
}