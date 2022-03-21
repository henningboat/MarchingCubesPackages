using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators
{
    public class TwistDecorator : MonoBehaviour
    {
        [SerializeField] private float _twist;

        public float Twist => _twist/100f;
    }
}