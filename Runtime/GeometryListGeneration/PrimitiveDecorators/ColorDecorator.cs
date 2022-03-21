using UnityEngine;
using UnityEngine.Serialization;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration.PrimitiveDecorators
{
    public class ColorDecorator:MonoBehaviour
    {
       [SerializeField]private Color _color = Color.white;

        public Color Color => _color;
    }
}