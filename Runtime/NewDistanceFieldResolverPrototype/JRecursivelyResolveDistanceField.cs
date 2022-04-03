﻿using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.NewDistanceFieldResolverPrototype
{
    [BurstCompile]
    public struct JRecursivelyResolveDistanceField : IJob
    {
        public NativeArray<PackedDistanceFieldData> GeometryFieldBuffer;

        private static readonly PackedFloat3 SubBlockOffsetKernelA =
            new(new PackedFloat(-0.5f, +0.5f, -0.5f, +0.5f),
                new PackedFloat(-0.5f, -0.5f, +0.5f, +0.5f),
                new PackedFloat(-0.5f, -0.5f, -0.5f, -0.5f));

        private static readonly PackedFloat3 SubBlockOffsetKernelB =
            new(new PackedFloat(-0.5f, +0.5f, -0.5f, +0.5f),
                new PackedFloat(-0.5f, -0.5f, +0.5f, +0.5f),
                new PackedFloat(+0.5f, +0.5f, +0.5f, +0.5f));

        public float Time;

        public void Execute()
        {
            var center = new PackedFloat3(32);
            float extends = 32;

            var newPositions = new NativeList<PackedFloat3>(Allocator.Temp);
            var previousPositions = new NativeList<PackedFloat3>(Allocator.Temp);

            previousPositions.Add(center + extends * SubBlockOffsetKernelA);
            previousPositions.Add(center + extends * SubBlockOffsetKernelB);

            while (extends > 1)
            {
                extends /= 2;

                newPositions.Clear();
                for (var i = 0; i < previousPositions.Length; i++)
                {
                    CheckAddChildPositions(newPositions, previousPositions[i], extends);
                }

                (previousPositions, newPositions) = (newPositions, previousPositions);
            }

            for (int i = 0; i < previousPositions.Length; i++)
            {
                previousPositions[i] -= 0.5f;
            }
            
            foreach (PackedFloat3 position in previousPositions)
            {
                ComputeDistanceAndWriteToBuffer(position);
            }

            Log(previousPositions);

            newPositions.Dispose();
            previousPositions.Dispose();
        }

        private void ComputeDistanceAndWriteToBuffer(PackedFloat3 position)
        {
            var distance = ComputeDistance(position);
            WriteToBufferPS(position,distance);
        }

        private void WriteBiggerBlockToBuffer(float3 position, float extends, float distanceValue)
        {
            for (float x = -extends; x < extends; x++)
            {
                for (float y = -extends; y < extends; y++)
                {
                    for (float z = -extends; z < extends; z++)
                    {
                        WriteToBufferSS(position + new float3(x, y, z), distanceValue);
                    }
                }
            }
        }

        private void WriteToBufferPS(PackedFloat3 position, PackedFloat distance)
        {
            for (int i = 0; i < 4; i++)
            {
                WriteToBufferSS(
                    new float3(position.x.PackedValues[i], position.y.PackedValues[i], position.z.PackedValues[i]),
                    distance.PackedValues[i]);
            }
        }

        private void WriteToBufferSS(float3 position, float distancePackedValue)
        {
            int3 positionFloored = (int3) math.floor(position);
            int index = DistanceFieldGeneration.Utils.PositionToIndex(positionFloored, Constants.clusterLength);
            var surfaceDistancePackedValues = GeometryFieldBuffer[index / 4];
            var surfaceDistance = surfaceDistancePackedValues.SurfaceDistance;
            surfaceDistance.PackedValues[index % 4] = distancePackedValue;
            GeometryFieldBuffer[index / 4] = new PackedDistanceFieldData() {SurfaceDistance = surfaceDistance};
        }

        private PackedFloat ComputeDistance(PackedFloat3 position)
        {
            return SimdMath.length(position-32) - 28;
        }

        [BurstDiscard]
        private static void Log(NativeList<PackedFloat3> previousPositions)
        {
            var allContained = new List<float3>();
            foreach (PackedFloat3 position in previousPositions)
            {
                for (int i = 0; i < 4; i++)
                {
                    allContained.Add(new float3(position.x.PackedValues[i],position.y.PackedValues[i],position.z.PackedValues[i]));
                }
            }
            Debug.Log(allContained.Count);

            Debug.Log(allContained.Distinct().ToList().Count.ToString());
        }


        private  void CheckAddChildPositions(NativeList<PackedFloat3> positionsToResolve, PackedFloat3 center,
            float extends)
        {
            //the *2 is placeholder
            var distanceAbs = (ComputeDistance(center));

            for (int i = 0; i < 4; i++)
            {
                var centerOfBlock = new float3(center.x.PackedValues[i], center.y.PackedValues[i],
                    center.z.PackedValues[i]);
                if (math.abs(distanceAbs.PackedValues[i]) < extends * 2.5f)
                {
                    var pos = new PackedFloat3(centerOfBlock);

                    positionsToResolve.Add(pos + extends * SubBlockOffsetKernelA);
                    positionsToResolve.Add(pos + extends * SubBlockOffsetKernelB);
                }
                else
                {
                    WriteBiggerBlockToBuffer(centerOfBlock, extends, distanceAbs.PackedValues[i]);
                }
            }
        }

        private void Resolve(float3 position, float width, NativeList<PackedFloat3> positionsToResolve)
        {
        }
    }
}