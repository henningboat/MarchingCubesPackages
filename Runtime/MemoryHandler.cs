using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace henningboat.CubeMarching
{
    internal static class MemoryHandler
    {
        private static List<IDisposable> _disposeOnRecompile = new();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void RegisterEvent()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () => { CleanupDisposables(); };
        }

        private static void CleanupDisposables()
        {
            Debug.Log($"Disposing {_disposeOnRecompile.Count} buffers");
            foreach (var disposable in _disposeOnRecompile) disposable.Dispose();
        }
#endif
    
        public static NativeArray<T> CreateNativeArray<T>(T[] data, Allocator allocator) where T : struct
        {
            var array = new NativeArray<T>(data, allocator);
            _disposeOnRecompile.Add(array);
            return array;
        }
    }
}