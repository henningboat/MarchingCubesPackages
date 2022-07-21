using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using SIMDMath;
using Unity.Mathematics;
using UnityEngine;
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

                // for (int j = 0; j < 8; j++)
                // {
                //     float3 position = new float3(positionOS.x.PackedValues[j],positionOS.y.PackedValues[j],positionOS.z.PackedValues[j]);
                //     if (surfaceDistance.PackedValues[j] <= 0)
                //     {
                //         Debug.DrawRay(position, Vector3.up * 0.2f);
                //     } 
                // }
            }
        }

        public ShapeType Type => ShapeType.Sphere;
    }
}