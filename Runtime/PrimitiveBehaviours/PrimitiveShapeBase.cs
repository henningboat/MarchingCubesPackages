using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public abstract class PrimitiveShapeBase : GeometryInstanceBase
    {
        protected override NewGeometryGraphData GetGeometryGraphData()
        {
            using (var context = new RuntimeGeometryGraphResolverContext())
            {
                var shapeProxy = GetShapeProxy(context);
                TransformationValue = shapeProxy.TransformationValue;
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

        protected abstract GeometryInstructionProxy GetShapeProxy(RuntimeGeometryGraphResolverContext context);
    }

    public abstract class ShapeProxy : GeometryInstructionProxy
    {
        public abstract ShapeType ShapeType { get; }
        public override int GeometryInstructionSubType => (int) ShapeType;
        public override GeometryInstructionType GeometryInstructionType => GeometryInstructionType.Shape;

        protected ShapeProxy(GeometryGraphProperty transformation) : base(transformation)
        {
        }
    }
}