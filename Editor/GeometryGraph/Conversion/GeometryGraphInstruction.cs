using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;

namespace Editor.GeometryGraph.Conversion
{
    public abstract class GeometryGraphInstruction
    {
        public readonly int Depth;

        protected GeometryGraphInstruction(int depth)
        {
            Depth = depth;
        }

        public abstract GeometryInstruction GetInstruction();
    }
}