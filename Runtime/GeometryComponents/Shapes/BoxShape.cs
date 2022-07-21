using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using Unity.Mathematics;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct BoxShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(8, 8, 8)]
        public float3 Extends;

        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            for (int i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                
                var extends = Extends;
                var q = abs(positionOS) - extends;
                var surfaceDistance = length(max(q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
                
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
        }

        public ShapeType Type => ShapeType.Box;
    }
}