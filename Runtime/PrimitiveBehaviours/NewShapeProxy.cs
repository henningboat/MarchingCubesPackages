using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class NewShapeProxy : GeometryInstanceBase
    {
        [SerializeField] private ShapeType _shapeType;
        [SerializeField] private NewGeometryGraphData _graphData;

#if UNITY_EDITOR
        public void Initialize(NewGeometryGraphData geometryGraphData)
        {
            _graphData = geometryGraphData;
        }
        #endif
        
        public override NewGeometryGraphData GeometryGraphData => _graphData;
    }
}