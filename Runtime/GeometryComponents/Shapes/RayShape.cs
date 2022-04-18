using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct RayShape : IGeometryShape
    {
        [FieldOffset(0)] [DefaultValue(0, 8.0f, 0)]
        public float3 direction;

        [FieldOffset(3 * 4)] [DefaultValue(3)] public float radius;

        public void WriteShape(GeometryInstructionIterator iterator, in BinaryDataStorage assetData,
            in GeometryInstruction instruction)
        {
            for (int i = 0; i < iterator.BufferLength; i++)
            {
                var positionOS = iterator.CalculatePositionWSFromInstruction(instruction, i);
                
                
                var h = SimdMath.clamp(SimdMath.dot(positionOS, direction) / SimdMath.dot(direction, direction), 0.0f,
                    1.0f);
                var surfaceDistance = SimdMath.length(positionOS - (PackedFloat3) direction * h) - radius;
                
                
                iterator.WriteDistanceField(i, surfaceDistance, instruction);
            }
            
            
           
        }

        public ShapeType Type => ShapeType.Ray;
    }
}