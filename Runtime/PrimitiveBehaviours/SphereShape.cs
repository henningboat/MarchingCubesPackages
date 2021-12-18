using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class SphereShape : PrimitiveShapeBase
    {
        protected override GeometryInstructionProxy GetShapeProxy(RuntimeGeometryGraphResolverContext context)
        {
            return new SphereShapeProxy(context.Constant(1), context.Constant(transform.localToWorldMatrix));
        }

    }
}