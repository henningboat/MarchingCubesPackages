using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators
{
    public class InflationDecorator : MonoBehaviour
    {
        [SerializeField] private float _inflation;

        public float Inflation => _inflation;
    }
}