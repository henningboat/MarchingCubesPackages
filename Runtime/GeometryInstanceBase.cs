using System;
using System.Collections.Generic;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEngine;

namespace henningboat.CubeMarching
{
    [ExecuteInEditMode]
    public abstract class GeometryInstanceBase : MonoBehaviour
    {
        private GeometryGraphBuffers _graphBuffer;
        [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites = new();
        public abstract NewGeometryGraphData GeometryGraphData { get; }

        private void OnDisable()
        {
            TryDisposeGraphBuffers();
        }


        private void UpdateOverwritesInValueBuffer()
        {
            foreach (var overwrite in _overwrites)
            {
                var variable = GeometryGraphData.GetIndexOfProperty(overwrite.PropertyGUID);

                if (variable == null)
                    continue;

                for (var i = 0; i < variable.GetSizeInBuffer(); i++)
                    _graphBuffer.ValueBuffer.Write(overwrite.Value[i], variable.Index + i);
            }

            _graphBuffer.ValueBuffer.Write(transform.worldToLocalMatrix, GeometryGraphData.MainTransformation.Index);
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
            if (GeometryGraphData == null)
            {
                TryDisposeGraphBuffers();
                result = default;
                return false;
            }

            if (_graphBuffer.ContentHash != GeometryGraphData.ContentHash)
            {
                TryDisposeGraphBuffers();
                _graphBuffer = new GeometryGraphBuffers(GeometryGraphData);
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