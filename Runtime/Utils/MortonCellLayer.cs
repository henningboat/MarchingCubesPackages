using System;
using System.Runtime.CompilerServices;
using SIMDMath;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Utils
{
    public struct MortonCellLayer
    {
        public readonly uint CellPackedBufferSize;
        private readonly uint _childCellSize;
        private readonly uint _cellLength;

        private static readonly PackedFloat3 childOffsets = new(
            new float3(0.0f, 0.0f, 0),
            new float3(0.5f, 0.0f, 0),
            new float3(0.0f, 0.5f, 0),
            new float3(0.5f, 0.5f, 0),
            new float3(0.0f, 0.0f, 0.5f),
            new float3(0.5f, 0.0f, 0.5f),
            new float3(0.0f, 0.5f, 0.5f),
            new float3(0.5f, 0.5f, 0.5f));

        public MortonCellLayer(uint cellLength)
        {
            _cellLength = cellLength;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!math.ispow2(cellLength))
                throw new ArgumentOutOfRangeException(nameof(cellLength), cellLength, "Length must be a power of two");
#endif

            CellLength = cellLength;

            CellPackedBufferSize = CellLength * CellLength * CellLength / 8;
            _childCellSize = CellPackedBufferSize;
        }

        public uint CellLength { get; set; }

        public MortonCoordinate GetChildCell(MortonCoordinate mortonCoordinate, uint childCellIndex)
        {
            return new MortonCoordinate(mortonCoordinate.MortonNumber + _childCellSize * childCellIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3 GetMortonCellChildPositions(MortonCoordinate mortonCoordinate)
        {
            PackedFloat3 childPositions = mortonCoordinate.GetPositionFloat3();
            childPositions += childOffsets * _cellLength;
            return childPositions;
        }

        public void DrawGizmos(MortonCoordinate mortonCoordinate)
        {
            Vector3 minPos = mortonCoordinate.GetPositionFloat3();
            Gizmos.DrawWireCube(minPos + Vector3.one * (0.5f * CellLength), Vector3.one * CellLength);
        }

        public MortonCellLayer GetChildLayer()
        {
            return new MortonCellLayer(_cellLength / 2);
        }
    }
}