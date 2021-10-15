using System;
using System.Collections.Generic;
using Unity.Collections;

internal static class MemoryHandler
{
    private static List<IDisposable> _disposeOnRecompile = new();

    public static NativeArray<T> CreateNativeArray<T>(T[] data, Allocator allocator) where T : struct
    {
        var array = new NativeArray<T>(data, allocator);
        _disposeOnRecompile.Add(array);
        return array;
    }
}