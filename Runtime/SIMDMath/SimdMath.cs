using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SIMDMath
{
    public static class SimdMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat abs(PackedFloat val)
        {
            return new(math.abs(val.PackedValues.a), math.abs(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat frac(PackedFloat val)
        {
            return new(math.frac(val.PackedValues.a), math.frac(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 frac(PackedFloat3 val)
        {
            return new(frac(val.x), frac(val.y), frac(val.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 abs(PackedFloat3 a)
        {
            return new(abs(a.x), abs(a.y), abs(a.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat min(PackedFloat lhs, PackedFloat rhs)
        {
            return new(math.min(lhs.PackedValues.a, rhs.PackedValues.a),
                math.min(lhs.PackedValues.b, rhs.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 min(PackedFloat3 a, PackedFloat3 b)
        {
            return new(min(a.x, b.x), min(a.y, b.y), min(a.z, b.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 min(PackedFloat3 a, PackedFloat rhs)
        {
            return new(min(a.x, rhs), min(a.y, rhs), min(a.z, rhs));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat max(PackedFloat lhs, PackedFloat rhs)
        {
            return new(math.max(lhs.PackedValues.a, rhs.PackedValues.a),
                math.max(lhs.PackedValues.b, rhs.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 max(PackedFloat3 a, PackedFloat rhs)
        {
            return new(max(a.x, rhs), max(a.y, rhs), max(a.z, rhs));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 max(PackedFloat3 a, PackedFloat3 b)
        {
            return new(max(a.x, b.x), max(a.y, b.y), max(a.z, b.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat floor(PackedFloat val)
        {
            return new PackedFloat(math.floor(val.PackedValues.a), math.floor(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat ceil(PackedFloat a)
        {
            return new PackedFloat(math.ceil(a.PackedValues.a), math.ceil(a.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 floor(PackedFloat3 a)
        {
            return new PackedFloat3(floor(a.x),floor(a.y),floor(a.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 ceil(PackedFloat3 a)
        {
            return new PackedFloat3(ceil(a.x),ceil(a.y),ceil(a.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat lerp(PackedFloat lhs, PackedFloat rhs, PackedFloat t)
        {
            return new(math.lerp(lhs.PackedValues.a, rhs.PackedValues.a, t.PackedValues.a),
                                math.lerp(lhs.PackedValues.b, rhs.PackedValues.b, t.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 lerp(PackedFloat3 a, PackedFloat3 b, PackedFloat t)
        {
            return new(lerp(a.x, b.x, t), lerp(a.y, b.y, t), lerp(a.z, b.z, t));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat dot(PackedFloat3 a, PackedFloat3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat dot(PackedFloat2 a, PackedFloat2 b)
        {
            return a.x * b.x + a.y * b.y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat sqrt(PackedFloat val)
        {
            val.PackedValues.a = math.sqrt(val.PackedValues.a);
            val.PackedValues.b = math.sqrt(val.PackedValues.b);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat length(PackedFloat3 a)
        {
            return sqrt(dot(a, a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat length(PackedFloat2 a)
        {
            return sqrt(dot(a, a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat clamp(PackedFloat val, PackedFloat min, PackedFloat max)
        {
            return new(math.clamp(val.PackedValues.a, min.PackedValues.a, max.PackedValues.a),
                math.clamp(val.PackedValues.b, min.PackedValues.b, max.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat clamp(PackedFloat val, float min, float max)
        {
            return clamp(val, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 PackedFloat3(PackedFloat x, PackedFloat y, PackedFloat z)
        {
            return new(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 PackedFloat2(PackedFloat x, PackedFloat y)
        {
            return new(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat acos(PackedFloat value)
        {
            return new(math.acos(value.PackedValues.a), math.acos(value.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat sin(PackedFloat val)
        {
            return new(math.sin(val.PackedValues.a), math.sin(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 sin(PackedFloat3 value)
        {
            return new PackedFloat3(sin(value.x), sin(value.y), sin(value.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat cos(PackedFloat val)
        {
            return new(math.cos(val.PackedValues.a), math.cos(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat atan(PackedFloat val)
        {
            return new(math.atan(val.PackedValues.a), math.atan(val.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat pow(PackedFloat x, PackedFloat y)
        {
            return new(math.pow(x.PackedValues.a, y.PackedValues.a),math.pow(x.PackedValues.b, y.PackedValues.b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat log(PackedFloat val)
        {
            return new(math.log(val.PackedValues.a), math.log(val.PackedValues.b));;
        }

        public static PackedFloat mod(PackedFloat val, PackedFloat periode)
        {
            return new PackedFloat(
                math.modf(val.PackedValues.a / periode.PackedValues.a, out float4 _) * periode.PackedValues.a,
                math.modf(val.PackedValues.b / periode.PackedValues.b, out float4 _) * periode.PackedValues.b);
        }
        
        public static PackedFloat3 mod(PackedFloat3 a, PackedFloat3 periode)
        {
            return new PackedFloat3(mod(a.x, periode.x), mod(a.y, periode.y), mod(a.z, periode.z));
        }

        public static bool any(bool8 aInside)
        {
            return math.any(aInside.a) | math.any(aInside.b);
        }

        public static PackedFloat @select(PackedFloat a, PackedFloat b, bool8 s)
        {
            return new PackedFloat(math.@select(a.PackedValues.a,b.PackedValues.a,s.a),
                                        math.@select(a.PackedValues.b,b.PackedValues.b,s.b));
        }
    }
}