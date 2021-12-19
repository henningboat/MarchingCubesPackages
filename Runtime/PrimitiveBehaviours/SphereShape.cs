using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class SphereShape : PrimitiveShapeBase
    {
        protected override GeometryInstructionProxy GetShapeProxy(RuntimeGeometryGraphResolverContext context)
        {
            return new SphereShapeProxy(context.CreateProperty(1.0f), context.CreateProperty(transform.localToWorldMatrix));
        }

    }
}