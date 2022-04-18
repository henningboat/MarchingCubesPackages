using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct SphereShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8.0f)] public float radius;

        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            for (int i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                var surfaceDistance = length(positionOS) - radius;
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
        }

        public ShapeType Type => ShapeType.Sphere;
    }
}