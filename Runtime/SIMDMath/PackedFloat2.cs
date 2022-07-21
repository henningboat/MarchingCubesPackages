using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat2
    {
        public bool Equals(PackedFloat2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }

        public PackedFloat x;
        public PackedFloat y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat2(float8 packedX, float8 packedY)
        {
            x = new PackedFloat(packedX);
            y = new PackedFloat(packedY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat2(PackedFloat x, PackedFloat y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator +(PackedFloat2 a, PackedFloat2 b)
        {
            return new(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator +(PackedFloat2 a, PackedFloat b)
        {
            return new(a.x + b, a.y + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator -(PackedFloat2 a, PackedFloat2 b)
        {
            return new(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator -(PackedFloat2 a, PackedFloat b)
        {
            return new(a.x - b, a.y - b);
        }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool operator ==(PackedFloat2 a, PackedFloat2 b)
        // {
        //     return (a.x == b.x) & (a.y == b.y);
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool8 operator !=(PackedFloat2 a, PackedFloat2 b)
        // {
        //     return !(a == b);
        // }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat2(float2 a)
        {
            return new((PackedFloat) a.x, a.y);
        }
    }
}