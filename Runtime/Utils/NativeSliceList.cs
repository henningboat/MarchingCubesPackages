using Unity.Collections;

namespace henningboat.CubeMarching.Utils
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
}