using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems.GenerationGraphSystem;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometrySystems
{
    [ExecuteInEditMode]
    public abstract class GeometryInstance : MonoBehaviour
    {
        private GeometryGraphBuffers _graphBuffer;
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
                    _graphBuffer.ValueBuffer.Write(overwrite.Value[i], variable.Index + i);
            }

            _graphBuffer.ValueBuffer.Write(transform.worldToLocalMatrix, GeometryInstructionList.MainTransformation.Index);
        }


        public List<GeometryGraphPropertyOverwrite> GetOverwrites()
        {
            return _overwrites;
        }

        public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
        {
            _overwrites = newOverwrites;
        }

        public bool TryInitializeAndGetBuffer(out GeometryGraphBuffers result)
        {
            if (GeometryInstructionList == null)
            {
                TryDisposeGraphBuffers();
                result = default;
                return false;
            }

            if (_graphBuffer.ContentHash != GeometryInstructionList.ContentHash)
            {
                TryDisposeGraphBuffers();
                _graphBuffer = new GeometryGraphBuffers(GeometryInstructionList);
            }

            result = _graphBuffer;

            UpdateOverwritesInValueBuffer();

            return true;
        }

        private void TryDisposeGraphBuffers()
        {
            if (_graphBuffer.IsValid)
            {
                _graphBuffer.Dispose();
                _graphBuffer = default;
            }
        }
    }
}