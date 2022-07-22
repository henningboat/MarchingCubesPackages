using System;
using System.Collections.Generic;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Utils
{
    public class GizmosInjector : MonoBehaviour
    {
        private static GizmosInjector _instance;

        private readonly List<Action> _callbacks = new();

        public static GizmosInjector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject(nameof(GizmosInjector)).AddComponent<GizmosInjector>();
                    _instance.gameObject.hideFlags = HideFlags.DontSaveInEditor;
                }

                return _instance;
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var callback in _callbacks)
                try
                {
                    callback.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            
            _callbacks.Clear();
        }
 
        public void RegisterCallback(Action action)
        {
            _callbacks.Add(action);
            if (_callbacks.Count > 100)
            {
                _callbacks.Clear();
            }
        }

        public void UnregisterCallback(Action action)
        {
            _callbacks.Remove(action);
        }
    }
}