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

        private readonly DynamicBuffer<GeometryInstruction> _combinerInstructions;
        private NativeArray<PackedFloat3> _postionsWS;
        private readonly int _combinerStackSize;
        private NativeArray<PackedFloat3> _postionStack;
        private NativeArray<bool> _hasWrittenToCurrentCombiner;
        private int _lastCombinerDepth;

        public int StackBaseOffset;
        private MortonCellLayer _mortonCellLayer;
        private NativeArray<MortonCoordinate> _mortonCoordinates;
        private BufferFromEntity<PackedDistanceFieldData> _packedDistanceFieldDataHandle;
        private readonly Entity _selfEntityPlaceholder;

        private bool _isPrepass;
        private readonly BufferFromEntity<CGeometryLayerChild> _getGeometryLayerChild;
        private readonly int _entityIndexInBuffer;
        public NativeArray<GeometryInstructionHash> _contentHashBuffer;

        #endregion

        #region Constructors

        public GeometryInstructionIterator(NativeArray<MortonCoordinate> mortonCoordinates,
            DynamicBuffer<GeometryInstruction> combinerInstructions,
            MortonCellLayer mortonCellLayer, PackedFloat3 chunkBasePosition,
            BufferFromEntity<PackedDistanceFieldData> packedDistanceFieldDataHandle, Entity selfEntityPlaceholder, NativeArray<PackedFloat3> positionWS, bool isPrepass, BufferFromEntity<CGeometryLayerChild> getGeometryLayerChild, int entityIndexInBuffer)
        {
            _isPrepass = isPrepass;
            _getGeometryLayerChild = getGeometryLayerChild;
            _entityIndexInBuffer = entityIndexInBuffer;

            if (isPrepass)
            {
                _contentHashBuffer = new NativeArray<GeometryInstructionHash>(positionWS.Length, Allocator.Temp);
            }
            else
            {
                _contentHashBuffer = default;
            }
            
            _packedDistanceFieldDataHandle = packedDistanceFieldDataHandle;
            _selfEntityPlaceholder = selfEntityPlaceholder;
            _mortonCoordinates = mortonCoordinates;
            _mortonCellLayer = mortonCellLayer;

            _combinerInstructions = combinerInstructions;
            //todo cache this between pre-pass and actual pass
            _combinerStackSize = 0;
            for (var i = 0; i < combinerInstructions.Length; i++)
                _combinerStackSize = max(_combinerInstructions[i].CombinerDepth, _combinerStackSize);

            //todo workaround. Remove this and see the exceptions
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
                PackedDistanceFieldData distanceFieldData = default;
                switch (geometryInstruction.GeometryInstructionType)
                {
                    case GeometryInstructionType.CopyLayer:

                        var layerEntity = geometryInstruction.ReferenceEntity;
                        var targetSlice = _terrainDataBuffer.Slice(StackBaseOffset, _postionsWS.Length);
                        //if (_mortonCellLayer.CellLength == 2)
                        //{
                        if (_isPrepass)
                        {
                            //todo add this
                            for (int i = 0; i < targetSlice.Length; i++)
                            {
                                targetSlice[i] = new PackedDistanceFieldData(0, default);
                            }
                        }
                        else
                        {
                            var chunkEntityInOtherLayer = _getGeometryLayerChild[layerEntity][_entityIndexInBuffer].ClusterEntity;
                            var readbackBuffer = _packedDistanceFieldDataHandle[chunkEntityInOtherLayer];
                            targetSlice.CopyFrom(readbackBuffer.AsNativeArray());   
                        }
                        //}
                        // if (_mortonCellLayer.CellLength == 2)
                        // {
                        //     for (var i = 0; i < _mortonCoordinates.Length; i++)
                        //     {
                        //         for (int j = 0; j < 2; j++)
                        //         {
                        //             distanceFieldData = readbackBuffer[(int)_mortonCoordinates[i].MortonNumber / 4 + j];
                        //
                        //
                        //             var existingData = _terrainDataBuffer[StackBaseOffset + i];
                        //             var combinedResult = TerrainChunkOperations.CombinePackedTerrainData(
                        //                 geometryInstruction.CombinerBlendOperation,
                        //                 geometryInstruction.CombinerBlendFactor,
                        //                 existingData, distanceFieldData);
                        //             _terrainDataBuffer[StackBaseOffset + i] = combinedResult;
                        //         }
                        //     }
                        // }
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