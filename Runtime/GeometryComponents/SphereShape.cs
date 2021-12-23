using henningboat.CubeMarching.PrimitiveBehaviours;

namespace henningboat.CubeMarching.GeometryComponents.Shapes
{
    public class SphereShape : PrimitiveShapeBase
    {
        protected override GeometryInstructionProxy GetShapeProxy(RuntimeGeometryGraphResolverContext context)
        {
            return new SphereShapeProxy(context.CreateProperty(1.0f), context.CreateProperty(transform.localToWorldMatrix));
        }

    }
}