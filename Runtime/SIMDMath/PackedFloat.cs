using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat
    {
        public float8 PackedValues;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator +(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues + b.PackedValues);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator +(PackedFloat a, float b)
        {
            return new PackedFloat(a.PackedValues + b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues - b.PackedValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a)
        {
            return new PackedFloat(-a.PackedValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a, float b)
        {
            return new PackedFloat(a.PackedValues - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator *(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues * b.PackedValues);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator /(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues / b.PackedValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator >(PackedFloat a, PackedFloat b)
        {
            return new bool8(a.PackedValues.a > b.PackedValues.a,a.PackedValues.b > b.PackedValues.b);
        }    
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator <(PackedFloat a, PackedFloat b)
        {
            return new bool8(a.PackedValues.a < b.PackedValues.a,a.PackedValues.b < b.PackedValues.b);
        }

        public bool Equals(PackedFloat other)
        {
            return PackedValues.Equals(other.PackedValues);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat other && Equals(other);
        }

        public override int GetHashCode()
        {
            return PackedValues.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float8 packedValues)
        {
            PackedValues = packedValues;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float value)
        {
            PackedValues = new float8(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float4 a, float4 b)
        {
            PackedValues = new float8(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float a, float b, float c, float d, float e, float f, float g, float h)
        {
            PackedValues = new float8(a, b, c, d, e, f, g, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator ==(PackedFloat a, PackedFloat b)
        {
            return a.PackedValues == b.PackedValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator !=(PackedFloat a, PackedFloat b)
        {
            return a.PackedValues != b.PackedValues;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat(float a)
        {
            return new PackedFloat(a);
        }

        public override string ToString()
        {
            return
                $"{PackedValues.a.x} | {PackedValues.a.y} | {PackedValues.a.z} | {PackedValues.a.w} | {PackedValues.b.x} | {PackedValues.b.y} | {PackedValues.b.z} | {PackedValues.b.w}";
        }
    }
}