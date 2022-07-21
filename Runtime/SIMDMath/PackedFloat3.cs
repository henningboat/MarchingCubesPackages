using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat3
    {
        public override string ToString()
        {
            string text="";
            for (int i = 0; i < 8; i++)
            {
                text += $"{x.PackedValues[i]}|{y.PackedValues[i]}|{z.PackedValues[i]}  ";
            }

            return text;
        }

        public bool Equals(PackedFloat3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                return hashCode;
            }
        }

        public PackedFloat x;
        public PackedFloat y;
        public PackedFloat z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(float8 packedX, float8 packedY, float8 packedZ)
        {
            x = new PackedFloat(packedX);
            y = new PackedFloat(packedY);
            z = new PackedFloat(packedZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(PackedFloat x, PackedFloat y, PackedFloat z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(float3 original)
        {
            x = original.x;
            y = original.y;
            z = original.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(float3 a, float3 b, float3 c, float3 d, float3 e, float3 f, float3 g, float3 h)
        {
            x = new PackedFloat(new float8(a.x, b.x, c.x, d.x, e.x, f.x, g.x, h.x));
            y = new PackedFloat(new float8(a.y, b.y, c.y, d.y, e.y, f.y, g.y, h.y));
            z = new PackedFloat(new float8(a.z, b.z, c.z, d.z, e.z, f.z, g.z, h.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator +(PackedFloat3 a, PackedFloat3 b)
        {
            return new(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator +(PackedFloat3 a, PackedFloat b)
        {
            return new(a.x + b, a.y + b, a.z + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat3 a, PackedFloat3 b)
        {
            return new(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat3 a, PackedFloat b)
        {
            return new(a.x - b, a.y - b, a.z - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat a, PackedFloat3 b)
        {
            return new(b.x - a, b.y - a, b.z - a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat3 a, PackedFloat3 b)
        {
            return new(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat3 a, PackedFloat b)
        {
            return new(a.x * b, a.y * b, a.z * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat a, PackedFloat3 b)
        {
            return new(a * b.x, a * b.y, a * b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator ==(PackedFloat3 a, PackedFloat3 b)
        {
            return (a.x == b.x) & (a.y == b.y) & (a.z == b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool8 operator !=(PackedFloat3 a, PackedFloat3 b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator %(PackedFloat3 a, PackedFloat3 b)
        {
            a.x.PackedValues %= b.x.PackedValues;
            a.y.PackedValues %= b.y.PackedValues;
            a.z.PackedValues %= b.z.PackedValues;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat3(float3 a)
        {
            return new((PackedFloat) a.x, a.y, a.z);
        }

        public float3[] ToArray()
        {
            float3[] array = new float3[8];
            for (int i = 0; i < 8; i++)
            {
                array[i] = new float3(x.PackedValues[i], y.PackedValues[i], z.PackedValues[i]);
            }

            return array;
        }
    }
}