using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEngine;

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public abstract class GeometryInstanceBase : MonoBehaviour
    {
        public GeometryGraphData GraphData { get; set; }
        protected GeometryGraphValue TransformationValue;

        private void OnEnable()
        {
            GraphData = GetGeometryGraphData();
        }

        protected abstract GeometryGraphData GetGeometryGraphData();

        private void OnDisable()
        {
            GraphData.Dispose();
        }
        

        protected void WriteTransformationToValueBuffer()
        {
            //apply main transformation
            GraphData.ValueBuffer.Write(transform.worldToLocalMatrix, TransformationValue.Index);
        }

        public abstract void InitializeGraphDataIfNeeded();
        public abstract void UpdateOverwrites();
    }
}