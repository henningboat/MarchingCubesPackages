using Unity.Entities;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Systems
{
    [ExecuteAlways]
    [UpdateAfter(typeof(SUpdateDistanceField))]
    public partial class SRenderGeometryLayers : SystemBase
    {
        protected override void OnUpdate()
        {
            //TODO
        }
    }
}