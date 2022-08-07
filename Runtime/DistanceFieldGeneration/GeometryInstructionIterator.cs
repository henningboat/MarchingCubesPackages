using System;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using henningboat.CubeMarching.Runtime.Utils;
using SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    /// <summary>
    ///     Calculates the TerrainData for a NativeArray world space positions
    /// </summary>
    public struct GeometryInstructionIterator
    {
        #region Public Fields

        [NativeDisableParallelForRestriction]public NativeArray<PackedDistanceFieldData> _terrainDataBuffer;

        #endregion

        #region Private Fields

        private readonly NativeArray<GeometryInstruction> _combinerInstructions;
        private NativeArray<PackedFloat3> _postionsWS;
        private readonly int _combinerStackSize;
        private NativeArray<PackedFloat3> _postionStack;
        private NativeArray<bool> _hasWrittenToCurrentCombiner;
        private int _lastCombinerDepth;

        public int StackBaseOffset;
        
        private bool _isPrepass;
        private ReadbackHandler _readbackHandler;
        public NativeArray<GeometryInstructionHash> _contentHashBuffer;

        #endregion

        #region Constructors

        internal GeometryInstructionIterator(NativeArray<MortonCoordinate> mortonCoordinates,
            NativeArray<GeometryInstruction> combinerInstructions,
            MortonCellLayer mortonCellLayer, NativeArray<PackedFloat3> positionWS, bool isPrepass, ReadbackHandler readbackHandler)
        {
            _isPrepass = isPrepass;
            _readbackHandler = readbackHandler;

            if (isPrepass)
            {
                _contentHashBuffer = new NativeArray<GeometryInstructionHash>(positionWS.Length, Allocator.Temp);
            }
            else
            {
                _contentHashBuffer = default;
            }

            _combinerInstructions = combinerInstructions;
            //todo cache this between pre-pass and actual pass
            _combinerStackSize = 0;
            for (var i = 0; i < combinerInstructions.Length; i++)
                _combinerStackSize = max(_combinerInstructions[i].CombinerDepth, _combinerStackSize);

            _combinerStackSize++;

            _postionsWS = positionWS;

            _terrainDataBuffer = new NativeArray<PackedDistanceFieldData>(_combinerStackSize * _postionsWS.Length, Allocator.Temp);
            _postionStack = new NativeArray<PackedFloat3>(_postionsWS.Length * _combinerStackSize, Allocator.Temp);

            _hasWrittenToCurrentCombiner = new NativeArray<bool>(_combinerStackSize, Allocator.Temp);

            _lastCombinerDepth = -1;
            StackBaseOffset = 0;
        }

        public int BufferLength => _postionsWS.Length;

        #endregion

        #region Public methods

        public void ProcessAllInstructions()
        {
            for (var i = 0; i < _combinerInstructions.Length; i++) ProcessInstruction(i);

            //In case we did not write any data to the terrain, we fill it with a dummy value
            if (_hasWrittenToCurrentCombiner[0] == false)
                for (var i = 0; i < _postionsWS.Length; i++)
                    _terrainDataBuffer[i] = new PackedDistanceFieldData(10,
                        new PackedTerrainMaterial(TerrainMaterial.GetDefaultMaterial()));
        }

        public void Dispose()
        {
            _terrainDataBuffer.Dispose();
            _postionStack.Dispose();
            _hasWrittenToCurrentCombiner.Dispose();
            _postionsWS.Dispose();

            if (_isPrepass)
            {
                _contentHashBuffer.Dispose();
            }
        }

        #endregion

        #region Private methods

        public void ProcessInstruction(int instructionIndex)
        {
            var geometryInstruction = _combinerInstructions[instructionIndex];


            if (geometryInstruction.CombinerDepth > _lastCombinerDepth)
                for (var combinerDepthToInitialize = max(0, _lastCombinerDepth + 1);
                     combinerDepthToInitialize <= geometryInstruction.CombinerDepth;
                     combinerDepthToInitialize++)
                {
                    _hasWrittenToCurrentCombiner[combinerDepthToInitialize] = false;

                    var positionsLength = _postionsWS.Length;

                    if (combinerDepthToInitialize == 0)
                        NativeArray<PackedFloat3>.Copy(_postionsWS, 0, _postionStack,
                            positionsLength * combinerDepthToInitialize, positionsLength);
                    else
                        //copy all positions from the previous level in the stack to the new one
                        NativeArray<PackedFloat3>.Copy(_postionStack, positionsLength * (combinerDepthToInitialize - 1),
                            _postionStack, positionsLength * combinerDepthToInitialize, positionsLength);
                }

            StackBaseOffset = _postionsWS.Length * geometryInstruction.CombinerDepth;

            if (geometryInstruction.GeometryInstructionType == GeometryInstructionType.DistanceModification)
            {
                var distanceModificationInstruction = geometryInstruction.GetDistanceModificationInstruction();

                for (var i = 0; i < _postionsWS.Length; i++)
                {
                    var surfaceDistance = _terrainDataBuffer[StackBaseOffset + i];
                    surfaceDistance.SurfaceDistance =
                        distanceModificationInstruction.GetSurfaceDistance(surfaceDistance.SurfaceDistance);
                    _terrainDataBuffer[StackBaseOffset + i] = surfaceDistance;
                }

                _lastCombinerDepth = geometryInstruction.CombinerDepth;
                
                if (_isPrepass)
                {
                    for (var i = 0; i < _postionsWS.Length; i++)
                    {
                        WriteGeometryInstructionHash(i, geometryInstruction);
                    }
                }
                return;
            }

            if (_hasWrittenToCurrentCombiner[geometryInstruction.CombinerDepth] == false &&
                geometryInstruction.WritesToDistanceField)
            {
                geometryInstruction.CombinerBlendOperation = CombinerOperation.Replace;

                _hasWrittenToCurrentCombiner[geometryInstruction.CombinerDepth] = true;
            }


            {
                PackedDistanceFieldData distanceFieldData;
                switch (geometryInstruction.GeometryInstructionType)
                {
                    case GeometryInstructionType.CopyLayer:
                        var targetSlice = _terrainDataBuffer.Slice(StackBaseOffset, _postionsWS.Length);
                        _readbackHandler.DoReadback(targetSlice, geometryInstruction, _contentHashBuffer);
                        break;
                    case GeometryInstructionType.Shape:
                        var shape = geometryInstruction.GetShapeInstruction();
                        shape.WriteShape(this, default, geometryInstruction);
                        break;
                    case GeometryInstructionType.Combiner:

                        for (var i = 0; i < _postionsWS.Length; i++)
                        {
                            distanceFieldData =
                                _terrainDataBuffer[(geometryInstruction.CombinerDepth + 1) * _postionsWS.Length + i];


                            var existingData = _terrainDataBuffer[StackBaseOffset + i];
                            var combinedResult = TerrainChunkOperations.CombinePackedTerrainData(
                                geometryInstruction.CombinerBlendOperation, geometryInstruction.CombinerBlendFactor,
                                distanceFieldData, existingData);
                            _terrainDataBuffer[StackBaseOffset + i] = combinedResult;
                        }
                        
                        if (_isPrepass)
                        {
                            for (var i = 0; i < _postionsWS.Length; i++)
                            {
                                WriteGeometryInstructionHash(i, geometryInstruction);
                            }
                        }
                        break;
                    case GeometryInstructionType.PositionModification:

                        for (var i = 0; i < _postionsWS.Length; i++)
                        {
                            var positionOS = CalculatePositionWSFromInstruction(geometryInstruction, i);
                            _postionStack[_postionsWS.Length * geometryInstruction.CombinerDepth + i] =
                                geometryInstruction
                                    .GetTerrainTransformation().TransformPosition(positionOS);
                        }


                        if (_isPrepass)
                        {
                            for (var i = 0; i < _postionsWS.Length; i++)
                            {
                                WriteGeometryInstructionHash(i, geometryInstruction);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _lastCombinerDepth = geometryInstruction.CombinerDepth;
        }

        public void WriteDistanceField(int i, PackedFloat surfaceDistance, GeometryInstruction geometryInstruction)
        {
            
            var materialData = geometryInstruction.GetMaterialData();
            var packedMaterialData = new PackedTerrainMaterial(materialData);

            PackedDistanceFieldData distanceFieldData;

            distanceFieldData = new PackedDistanceFieldData(surfaceDistance, packedMaterialData);


            int indexInGeometryField = StackBaseOffset + i;

            var existingData = _terrainDataBuffer[indexInGeometryField];
            var combinedResult = TerrainChunkOperations.CombinePackedTerrainData(
                geometryInstruction.CombinerBlendOperation, geometryInstruction.CombinerBlendFactor,
                distanceFieldData, existingData);
            _terrainDataBuffer[indexInGeometryField] = combinedResult;

            if (_isPrepass)
            {
                if (SimdMath.any(SimdMath.abs(surfaceDistance) < 10))
                {
                    WriteGeometryInstructionHash(i, geometryInstruction);
                }
            }
        }

        private void WriteGeometryInstructionHash(int i, GeometryInstruction geometryInstruction)
        {
            var geometryInstructionHash = _contentHashBuffer[i];
            geometryInstructionHash.Append(ref geometryInstruction.GeometryInstructionHash);
            _contentHashBuffer[i] = geometryInstructionHash;
        }

        public PackedFloat3 CalculatePositionWSFromInstruction(GeometryInstruction geometryInstruction, int i)
        {
            var transformation = geometryInstruction.GetTransformation();

            PackedFloat3 positionOS = default;


            var positionWSValue = _postionStack[_postionsWS.Length * geometryInstruction.CombinerDepth + i];

            //todo add SIMD version
            for (var j = 0; j < 8; j++)
            {
                var positionWSSlice = new float4(positionWSValue.x.PackedValues[j], positionWSValue.y.PackedValues[j],
                    positionWSValue.z.PackedValues[j], 1);
                var positionOSSlice = mul(transformation, positionWSSlice);
                positionOS.x.PackedValues[j] = positionOSSlice.x;
                positionOS.y.PackedValues[j] = positionOSSlice.y;
                positionOS.z.PackedValues[j] = positionOSSlice.z;
            }

            return positionOS;
        }

        #endregion
    }
}