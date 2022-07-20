using System;
using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Entities;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

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

        
        public GeometryLayerAsset geometryLayerAsset => _geometryLayer;

        public CombinerOperation combinerOperation => _combinerOperation;

        private void OnDisable()
        {
            TryDisposeGraphBuffers();
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

        public void SetOverwrite(string propertyName, float32 value)
        {
            var variable = GeometryInstructionList.GetIndexOfProperty(propertyName);
            SetOverwrite(variable.ID, value);
        }

        private void SetOverwrite(SerializableGUID propertyGuid, float32 value)
        {
            foreach (var propertyOverwrite in _overwrites)
                if (propertyOverwrite.PropertyGUID == propertyGuid)
                {
                    propertyOverwrite.Value = value;
                    return;
                }

            _overwrites.Add(new GeometryGraphPropertyOverwrite(propertyGuid) {Value = value});
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