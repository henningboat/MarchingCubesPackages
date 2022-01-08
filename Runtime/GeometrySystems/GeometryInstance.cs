using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [ExecuteInEditMode]
    public abstract class GeometryInstance : MonoBehaviour
    {
        [SerializeField] private GeometryLayer _geometryLayer;
        private GeometryInstructionListBuffers _instructionListBuffer;

        // ReSharper disable once InconsistentNaming
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites = new();
        public abstract GeometryInstructionList GeometryInstructionList { get; }

        private void OnDisable()
        {
            TryDisposeGraphBuffers();
        }

        private void UpdateOverwritesInValueBuffer()
        {
            foreach (var overwrite in _overwrites)
            {
                var variable = GeometryInstructionList.GetIndexOfProperty(overwrite.PropertyGUID);

                if (variable == null)
                    continue;

                for (var i = 0; i < variable.GetSizeInBuffer(); i++)
                    _instructionListBuffer.ValueBuffer.Write(overwrite.Value[i], variable.Index + i);
            }

            _instructionListBuffer.ValueBuffer.Write(transform.worldToLocalMatrix,
                GeometryInstructionList.MainTransformation.Index);
        }


        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return _overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }

        public bool TryInitializeAndGetBuffer(out GeometryInstructionListBuffers result)
        {
            if (GeometryInstructionList == null)
            {
                TryDisposeGraphBuffers();
                result = default;
                return false;
            }

            if (_instructionListBuffer.ContentHash != GeometryInstructionList.ContentHash || _instructionListBuffer.TargetLayerID != TargetLayerID)
            {
                TryDisposeGraphBuffers();
                _instructionListBuffer = new GeometryInstructionListBuffers(GeometryInstructionList, TargetLayerID);
            }

            result = _instructionListBuffer;

            UpdateOverwritesInValueBuffer();

            return true;
        }

        public SerializableGUID TargetLayerID
        {
            get
            {
                if (_geometryLayer != null) return _geometryLayer.GeometryLayerID;

                return default;
            }
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