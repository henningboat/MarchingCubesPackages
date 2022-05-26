using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CLayerMeshData : ISharedComponentData,IEquatable<CLayerMeshData>,IDisposable
    {
        public LayerMeshData Value;

        public int testCounter;

        public bool Equals(CLayerMeshData other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is CLayerMeshData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public void Dispose()
        {
            Value?.Dispose();
        }
    }
    
    public class LayerMeshData:IDisposable
    {
        public ComputeBuffer ArgsBuffer;
        public ComputeBuffer TrianglePositionCountBuffer;
        public ComputeBuffer ChunksToTriangulize;
        public ComputeBuffer ChunksWithTriangles;
        public ComputeBuffer TriangleCountPerSubChunk;
        public ComputeBuffer IndexBufferCounter;
        public ComputeBuffer TriangleBuffer;
        public ComputeBuffer TriangulationIndices;
        public MaterialPropertyBlock PropertyBlock;
        public int3 VoxelCounts;

        public void Dispose()
        {
            ArgsBuffer.Dispose();
            ChunksToTriangulize.Dispose();
            TriangleCountPerSubChunk.Dispose();
            IndexBufferCounter.Dispose();
            TriangleBuffer.Dispose();
            TriangulationIndices.Dispose();
            ChunksWithTriangles.Dispose();
        }
    }
}