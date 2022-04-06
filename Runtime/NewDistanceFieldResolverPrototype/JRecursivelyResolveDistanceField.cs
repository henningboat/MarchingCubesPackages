using System;
using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
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
        public NativeArray<PackedDistanceFieldData> GeometryFieldBuffer;

        public float Time;

        public void Execute()
        {
            var newPositions = new NativeList<MortonCoordinate>(Allocator.Temp);
            var previousPositions = new NativeList<MortonCoordinate>(Allocator.Temp);

            previousPositions.Add(new MortonCoordinate(0));
            
            MortonCellLayer layer = new MortonCellLayer(64);
            
            while (layer.CellLength > 2)
            {
                newPositions.Clear();
                for (var parentCellIndex = 0; parentCellIndex < previousPositions.Length; parentCellIndex++)
                {
                    var parentPosition = previousPositions[parentCellIndex];
                    
                    void ResolveChildCells(bool secondRow,
                        NativeArray<PackedDistanceFieldData> buffer)
                    {
                        var childPositions = layer.GetMortonCellChildPositions(parentPosition, secondRow);
                        var distance = ComputeDistancePS(childPositions);
                        bool4 distanceInRange = SimdMath.abs(distance).PackedValues < (layer.CellLength * 1.25F);

                        for (uint i = 0; i < 4; i++)
                        {
                            var childMortonNumber = layer.GetChildCell(parentPosition, secondRow ? i + 4 : i);
                            if (distanceInRange[(int)i])
                            {
                                newPositions.Add(childMortonNumber);
                            }
                            else
                            {
                                WriteDistanceToCellInBuffer(buffer, childMortonNumber, layer, distance.PackedValues[(int)i]);
                            }
                        }
                    }

                    ResolveChildCells(false,GeometryFieldBuffer);
                    ResolveChildCells(true,GeometryFieldBuffer);
                }

                layer = layer.GetChildLayer();

                (previousPositions, newPositions) = (newPositions, previousPositions);
            }

            foreach (MortonCoordinate position in previousPositions)
            {
                ComputeDistanceAndWriteToBuffer(layer, position);
            }

            newPositions.Dispose();
            previousPositions.Dispose();
        }

        private static void WriteDistanceToCellInBuffer(NativeArray<PackedDistanceFieldData> buffer,
            MortonCoordinate childMortonNumber, MortonCellLayer layer, float distancePackedValue)
        {
            var distanceFieldData = new PackedDistanceFieldData(distancePackedValue);
            for (int i = 0; i < layer.CellPackedBufferSize / 8; i++)
            {
                buffer[(int) (childMortonNumber.MortonNumber / 4 + i)] = distanceFieldData;
            }
        }

        private void ComputeDistanceAndWriteToBuffer(MortonCellLayer layer, MortonCoordinate cellToResolve)
        {
            if (layer.CellLength != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(layer));
            }

            WriteRow(false, GeometryFieldBuffer);
            WriteRow(true, GeometryFieldBuffer);

            void WriteRow(bool secondRow, NativeArray<PackedDistanceFieldData> packedDistanceFieldDatas)
            {
                var positionWS = layer.GetMortonCellChildPositions(cellToResolve, secondRow);
                var distance = ComputeDistancePS(positionWS);

                var packedIndex = (int) cellToResolve.MortonNumber/4;
                if (secondRow)
                {
                    packedIndex++;
                }

                if (packedIndex >= packedDistanceFieldDatas.Length)
                {
                    return;
                }

                packedDistanceFieldDatas[packedIndex] = new PackedDistanceFieldData(distance);               
            }
        }

        private static PackedFloat ComputeDistancePS(PackedFloat3 position)
        {
            return SimdMath.length(position) - 40.5f;
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

        
        private static void WriteToBufferSS(float3 position, float distancePackedValue, NativeArray<PackedDistanceFieldData> buffer)
        {
            int3 positionFloored = (int3) math.floor(position);
            int index = DistanceFieldGeneration.Utils.PositionToIndex(positionFloored, Constants.clusterLength);
            var surfaceDistancePackedValues = buffer[index / 4];
            var surfaceDistance = surfaceDistancePackedValues.SurfaceDistance;
            surfaceDistance.PackedValues[index % 4] = distancePackedValue;
            buffer[index / 4] = new PackedDistanceFieldData() {SurfaceDistance = surfaceDistance};
        }

        // private  void CheckAddChildPositions(NativeList<PackedFloat3> positionsToResolve, PackedFloat3 center,
        //     float extends)
        // {
        //     //the *2 is placeholder
        //     var distanceAbs = (ComputeDistance(center));
        //
        //     for (int i = 0; i < 4; i++)
        //     {
        //         var centerOfBlock = new float3(center.x.PackedValues[i], center.y.PackedValues[i],
        //             center.z.PackedValues[i]);
        //         if (math.abs(distanceAbs.PackedValues[i]) < extends * 2.5f)
        //         {
        //             var pos = new PackedFloat3(centerOfBlock);
        //
        //             positionsToResolve.Add(pos + extends * SubBlockOffsetKernelA);
        //             positionsToResolve.Add(pos + extends * SubBlockOffsetKernelB);
        //         }
        //         else
        //         {
        //             WriteBiggerBlockToBuffer(centerOfBlock, extends, distanceAbs.PackedValues[i]);
        //         }
        //     }
        //}

        private void Resolve(float3 position, float width, NativeList<PackedFloat3> positionsToResolve)
        {
        }
    }
}