using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class NewShapeProxy : GeometryInstanceBase
    {
        [SerializeField] private ShapeType _shapeType;

        protected override NewGeometryGraphData GetGeometryGraphData()
        {
            using (var context = new RuntimeGeometryGraphResolverContext())
            {
                var shapeProxy = new GenericShapeProxy(_shapeType, context.OriginTransformation);
                TransformationValue = shapeProxy.TransformationValue;

                var guid = new SerializableGUID(new Hash128(1, 2, 3, 4).ToString());

                context.CreateOrGetExposedProperty(guid, 4.0f);
                context.AddShape(shapeProxy);
                return context.GetGeometryGraphData();
            }
        }

        public override void InitializeGraphDataIfNeeded()
        {
        }

        public override void UpdateOverwrites()
        {
            WriteTransformationToValueBuffer();
        }
    }
}