using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEngine;

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public abstract class GeometryInstanceBase : MonoBehaviour
    {
        public GeometryGraphBuffers GraphBuffers { get; set; }
        protected GeometryGraphProperty TransformationValue;

        private void OnEnable()
        {
            GraphBuffers = new GeometryGraphBuffers(GetGeometryGraphData());
        }

        protected abstract NewGeometryGraphData GetGeometryGraphData();

        private void OnDisable()
        {
            GraphBuffers.Dispose();
        }
        

        protected void WriteTransformationToValueBuffer()
        {
            //apply main transformation
            GraphBuffers.ValueBuffer.Write(transform.worldToLocalMatrix, TransformationValue.Index);
        }

        public abstract void InitializeGraphDataIfNeeded();
        public abstract void UpdateOverwrites();
    }
}