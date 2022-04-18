using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct TorusShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8.0f)] public float radius;
        [FieldOffset(4)] [DefaultValue(2.0f)] public float thickness;

        //SDF code from
        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        private PackedFloat sdTorus(PackedFloat3 p, PackedFloat radius, PackedFloat thickness)
        {
            var q = new PackedFloat2(SimdMath.length(new PackedFloat2(p.x, p.z)) - radius, p.y);
            return SimdMath.length(q) - thickness;
        }

        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            for (var i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                var surfaceDistance = sdTorus(positionOS, radius, thickness);
                ;
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
        }

        public ShapeType Type => ShapeType.Torus;
    }
}