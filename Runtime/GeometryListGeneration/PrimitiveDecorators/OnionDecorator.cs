using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators
{
    public class OnionDecorator : MonoBehaviour
    {
        [SerializeField] private float _thickness;

        public float3 Thickness => _thickness;
    }
}