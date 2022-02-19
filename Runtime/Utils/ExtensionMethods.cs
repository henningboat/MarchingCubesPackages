using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Utils
{
    internal static class ExtensionMethods
    {
        public static void DeleteSafe(this Object @object)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(@object);
            }
            else
            {
                Object.DestroyImmediate(@object);
            }
        }
    }
}