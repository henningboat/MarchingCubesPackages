﻿using Rendering;
using Unity.Collections;

namespace Utils
{
    public struct NativeSliceList<T> where T : struct
    {
        private NativeSlice<T> _slice;
        private int _count;
        public NativeSliceList(NativeArray<T> nativeArray, int offset, int capacity)
        {
            _slice=nativeArray.Slice(offset, capacity);
            _count = 0;
        }

        public int Count => _count;

        public void Add(T element)
        {
            _slice[_count] = element;
            _count++;
        }
    }

    public static class NativeSliceListExtensions
    {
        public static NativeSliceList<T> SliceList<T>(this NativeArray<T> array, int offset, int capacity) where T : struct
        {
            return new(array, offset, capacity);
        }
    }
}