﻿using Unity.Collections;

namespace Utils
{
    public static class NativeSliceListExtensions
    {
        public static NativeSliceList<T> SliceList<T>(this NativeArray<T> array, int offset, int capacity) where T : struct
        {
            return new(array, offset, capacity);
        }
    }
}