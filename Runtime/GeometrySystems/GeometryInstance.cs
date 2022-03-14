using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [ExecuteInEditMode]
    public abstract class GeometryInstance : MonoBehaviour
    {
        [SerializeField] private GeometryLayerAsset _geometryLayer;
        [SerializeField] private CombinerOperation _combinerOperation;

        // ReSharper disable once InconsistentNaming
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites = new();

        private GeometryInstructionListBuffers _instructionListBuffer;
        public abstract GeometryInstructionList GeometryInstructionList { get; }

        public GeometryLayer TargetLayer
        {
            get
            {
                if (_geometryLayer != null) return _geometryLayer.GeometryLayer;

                return default;
            }
        }

        public int Order
        {
            get
            {
                switch (_combinerOperation)
                {
                    case CombinerOperation.Min:
                        return 0;
                    case CombinerOperation.SmoothMin:
                        return 5;
                    case CombinerOperation.Subtract:
                        return 10;
                    case CombinerOperation.SmoothSubtract:
                        return 20;
                    case CombinerOperation.Max:
                        return 30;
                    case CombinerOperation.Add:
                        return 40;
                    case CombinerOperation.Replace:
                    case CombinerOperation.ReplaceMaterial:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnDisable()
        {
            TryDisposeGraphBuffers();
        }

        private void UpdateOverwritesInValueBuffer(GeometryFieldManager geometryFieldManager)
        {
            foreach (var overwrite in _overwrites)
            {
                var variable = GeometryInstructionList.GetIndexOfProperty(overwrite.PropertyGUID);

                if (variable == null)
                    continue;

                if (variable.IsAsset)
                {
                    int index = geometryFieldManager.EnsureAssetIsRegistered(overwrite.ObjectValue);
                    _instructionListBuffer.ValueBuffer.Write((float)index, variable.Index);
                }
                else
                {

                    for (var i = 0; i < variable.GetSizeInBuffer(); i++)
                        _instructionListBuffer.ValueBuffer.Write(overwrite.Value[i], variable.Index + i);
                }
            }

            _instructionListBuffer.ValueBuffer.Write(transform.worldToLocalMatrix,
                GeometryInstructionList.MainTransformation.Index);

            _instructionListBuffer.SetTopLevelBlendOperation(_combinerOperation);
        }


        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return _overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }

        public void SetOverwrite(int propertyIndex, float32 value)
        {
            _overwrites[propertyIndex].Value = value;
        }

        public bool TryInitializeAndGetBuffer(out GeometryInstructionListBuffers result,
            GeometryFieldManager geometryFieldManager)
        {
            if (GeometryInstructionList == null)
            {
                TryDisposeGraphBuffers();
                result = default;
                return false;
            }

            if (_instructionListBuffer.ContentHash != GeometryInstructionList.ContentHash ||
                _instructionListBuffer.TargetLayer != TargetLayer)
            {
                TryDisposeGraphBuffers();
                _instructionListBuffer = new GeometryInstructionListBuffers(GeometryInstructionList, TargetLayer);
            }

            result = _instructionListBuffer;

            UpdateOverwritesInValueBuffer(geometryFieldManager);

            return true;
        }

        private void TryDisposeGraphBuffers()
        {
            if (_instructionListBuffer.IsValid)
            {
                _instructionListBuffer.Dispose();
                _instructionListBuffer = default;
            }
        }
    }
}