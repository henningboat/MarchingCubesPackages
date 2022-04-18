using System;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;
using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
using henningboat.CubeMarching.Runtime.TerrainChunkSystem;
using SIMDMath;
using Unity.Collections;
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

        public NativeArray<PackedDistanceFieldData> _terrainDataBuffer;

        #endregion

        #region Private Fields

        private readonly NativeArray<GeometryInstruction>.ReadOnly _combinerInstructions;
        private NativeArray<PackedFloat3> _postionsWS;
        private readonly int _combinerStackSize;
        private NativeArray<PackedFloat3> _postionStack;
        private NativeArray<bool> _hasWrittenToCurrentCombiner;
        private int _lastCombinerDepth;

        private DistanceDataReadbackCollection _readbackLayers;
        
        public int StackBaseOffset;

        #endregion

        #region Constructors

        public GeometryInstructionIterator(NativeArray<PackedFloat3> positions, NativeArray<PackedDistanceFieldData> distanceFieldDatas,
            NativeArray<GeometryInstruction> combinerInstructions,
            DistanceDataReadbackCollection readbackLayers)
        {
            _readbackLayers = readbackLayers;

            _combinerInstructions = combinerInstructions.AsReadOnly();
            //todo cache this between pre-pass and actual pass
            _combinerStackSize = 0;
            for (var i = 0; i < combinerInstructions.Length; i++)
                _combinerStackSize = max(_combinerInstructions[i].CombinerDepth, _combinerStackSize);

            //todo workaround. Remove this and see the exceptions
            _combinerStackSize++;

            _postionsWS = positions;
            
            _terrainDataBuffer = new NativeArray<PackedDistanceFieldData>(_combinerStackSize * _postionsWS.Length, Allocator.Temp);
            _postionStack = new NativeArray<PackedFloat3>(_postionsWS.Length * _combinerStackSize, Allocator.Temp);

            _hasWrittenToCurrentCombiner = new NativeArray<bool>(_combinerStackSize, Allocator.Temp);

            _lastCombinerDepth = -1;
            StackBaseOffset = 0;
        }

        public int BufferLength => _postionsWS.Length;

        #endregion

        #region Public methods

        public void CalculateAllTerrainData()
        {
            for (var i = 0; i < _combinerInstructions.Length; i++) ProcessTerrainData(i);

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
        }

        #endregion

        #region Private methods

        public void ProcessTerrainData(int instructionIndex)
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
                        throw new NotImplementedException();
                        // for (var i = 0; i < _postionsWS.Length; i++)
                        // {
                        //     PackedFloat readbackSurfaceDistance = default;
                        //     PackedTerrainMaterial readbackPackedMaterialData = default;
                        //
                        //     for (var j = 0; j < 4; j++)
                        //     {
                        //         var indexInCluster = _inicesInCluster[i * 4 + j] +
                        //                              Constants.clusterVolume * _clusterIndex;
                        //         var readback =
                        //             _readbackLayers[geometryInstruction.GeometryInstructionSubType]
                        //                 .GeometryBuffer[indexInCluster / 4];
                        //
                        //         readbackSurfaceDistance.PackedValues[j] =
                        //             readback.SurfaceDistance.PackedValues[indexInCluster % 4];
                        //         readbackPackedMaterialData[j] = readback.TerrainMaterial[indexInCluster % 4];
                        //     }
                        //
                        //     distanceFieldData =
                        //         new PackedDistanceFieldData(readbackSurfaceDistance, readbackPackedMaterialData);
                        //
                        //
                        //     var existingData = _terrainDataBuffer[StackBaseOffset + i];
                        //     var combinedResult = TerrainChunkOperations.CombinePackedTerrainData(
                        //         geometryInstruction.CombinerBlendOperation, geometryInstruction.CombinerBlendFactor,
                        //         distanceFieldData, existingData);
                        //     _terrainDataBuffer[StackBaseOffset + i] = combinedResult;
                        // }
                        //
                        // break;
                    case GeometryInstructionType.Shape:
                        var shape = geometryInstruction.GetShapeInstruction();
                        shape.WriteShape(this, in _readbackLayers.BinaryDataStorage, geometryInstruction);
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

                        break;
                    case GeometryInstructionType.PositionModification:

                        for (var i = 0; i < _postionsWS.Length; i++)
                        {
                            var positionOS = CalculatePositionWSFromInstruction(geometryInstruction, i);
                            _postionStack[_postionsWS.Length * geometryInstruction.CombinerDepth + i] =
                                geometryInstruction
                                    .GetTerrainTransformation().TransformPosition(positionOS);
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
        }

        public PackedFloat3 CalculatePositionWSFromInstruction(GeometryInstruction geometryInstruction, int i)
        {
            var transformation = geometryInstruction.GetTransformation();

            PackedFloat3 positionOS = default;


            var positionWSValue = _postionStack[_postionsWS.Length * geometryInstruction.CombinerDepth + i];

            //todo add SIMD version
            for (var j = 0; j < 4; j++)
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