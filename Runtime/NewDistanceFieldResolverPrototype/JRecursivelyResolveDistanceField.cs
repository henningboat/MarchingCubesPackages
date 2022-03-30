using System;
using System.Linq;
using henningboat.CubeMarching.Runtime.Utils;
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
        static readonly PackedFloat3 subBlockOffsetKernelA =
            new(new PackedFloat(-0.5f, +0.5f, -0.5f, +0.5f),
            new PackedFloat(-0.5f, -0.5f, +0.5f, +0.5f),
            new PackedFloat(-0.5f, -0.5f, -0.5f, -0.5f));

        static readonly PackedFloat3 subBlockOffsetKernelB = 
            new(new PackedFloat(-0.5f, +0.5f, -0.5f, +0.5f),
            new PackedFloat(-0.5f, -0.5f, +0.5f, +0.5f),
            new PackedFloat(+0.5f, +0.5f, +0.5f, +0.5f));

        public void Execute()
        {
            PackedFloat3 center = new PackedFloat3(4);
            float extends = 4;

            NativeList<PackedFloat3> newPositions = new NativeList<PackedFloat3>(Allocator.Temp);
            NativeList<PackedFloat3> previousPositions = new NativeList<PackedFloat3>(Allocator.Temp);

            previousPositions.Add(center + (extends) * subBlockOffsetKernelA);
            previousPositions.Add(center + (extends) * subBlockOffsetKernelB);
            
            while (extends >= 1)
            {
                extends /= 2;
                
                newPositions.Clear();
                for (var i = 0; i < previousPositions.Length; i++)
                {
                    var position = previousPositions[i];
                    AddChildPositions(newPositions, position, extends);
                }

                (previousPositions, newPositions) = (newPositions, previousPositions);
            }
 
            Log(previousPositions);

            newPositions.Dispose();
            previousPositions.Dispose();
        }

        [BurstDiscard]
        private static void Log(NativeList<PackedFloat3> previousPositions)
        {
            Debug.Log(previousPositions.Length);

            Debug.Log(previousPositions.ToArray().Distinct().ToList().Count.ToString());
        }


        private static void AddChildPositions(NativeList<PackedFloat3> positionsToResolve, PackedFloat3 center, float extends)
        {
            positionsToResolve.Add(center + extends * subBlockOffsetKernelA);
            positionsToResolve.Add(center + extends * subBlockOffsetKernelB);
        }

        private void Resolve(float3 position, float width, NativeList<PackedFloat3> positionsToResolve)
        {

        }
    }
}