using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators
{
    public class RepetitionDecorator : MonoBehaviour
    {
        [SerializeField] private float3 _size;

        public float3 Size => _size;
    }
}