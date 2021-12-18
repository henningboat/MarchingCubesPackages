namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class SphereShape : PrimitiveShapeBase
    {
        protected override GeometryInstructionProxy GetShapeProxy()
        {
            return new SphereShapeProxy(PrimitiveBuilder.Constant(1));
        }
    }
}