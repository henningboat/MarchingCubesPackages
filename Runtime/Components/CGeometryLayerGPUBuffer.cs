using System;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CGeometryLayerGPUBuffer : ISharedComponentData, IEquatable<CGeometryLayerGPUBuffer>, IDisposable
    {
        public GeometryLayerGPUBuffer Value;

        public bool Equals(CGeometryLayerGPUBuffer other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is CGeometryLayerGPUBuffer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public void Dispose()
        {
            Value?.Dispose();
        }
    }

    public class GeometryLayerGPUBuffer : ISharedComponentData, IDisposable
    {
        public ComputeBuffer DistanceFieldBuffer;
        public ComputeBuffer IndexMapBuffer;
        public ComputeBuffer ChunkBasePositions;
        public int3 ChunkCounts;

        //placeholder
        public int RegisteredChunksCount;

        public GeometryLayerGPUBuffer(int3 clusterCounts)
        {
            DistanceFieldBuffer = new ComputeBuffer(clusterCounts.Volume() * Constants.chunkVolume / 4, 4 * 4 * 2,
                ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            IndexMapBuffer = new ComputeBuffer(clusterCounts.Volume(), 4, ComputeBufferType.Structured,
                ComputeBufferMode.SubUpdates); 
            ChunkBasePositions = new ComputeBuffer(clusterCounts.Volume(), 4, ComputeBufferType.Structured,
                ComputeBufferMode.SubUpdates);
            RegisteredChunksCount = 0;
            ChunkCounts = clusterCounts;
        }

        public void Dispose()
        {
            DistanceFieldBuffer?.Dispose();
            DistanceFieldBuffer = null;
            IndexMapBuffer?.Dispose();
            IndexMapBuffer = null;
            ChunkBasePositions?.Dispose();
            ChunkBasePositions = null;
        }

        public CGeometryChunkGPUIndices RegisterChunkEntity(Entity chunkEntity)
        {
            var cGeometryChunkGPUIndices = new CGeometryChunkGPUIndices
            {
                DistanceFieldBufferOffset = RegisteredChunksCount
            };

            RegisteredChunksCount++;

            return cGeometryChunkGPUIndices;
        }
    }
}