using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static unsafe class DynamicBufferFloatExtensionMethods
{
    public static T Read<T>(this NativeArray<float> buffer, int index) where T : struct
    {
        return UnsafeUtility.ReadArrayElementWithStride<T>(buffer.GetUnsafeReadOnlyPtr(), 1, sizeof(float) * index);
    }

    public static void Write<T>(this NativeArray<float> buffer, T value, int index) where T : struct
    {
        UnsafeUtility.WriteArrayElementWithStride(buffer.GetUnsafePtr(), 1, sizeof(float) * index, value);
    }
}