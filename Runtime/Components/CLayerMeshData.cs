using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.Components
{
    public struct CLayerMeshData : ISharedComponentData,IEquatable<CLayerMeshData>,IDisposable
    {
        public LayerMeshData Value;

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
        public ComputeBuffer ChunksToTriangulate;
        public ComputeBuffer ChunksWithTriangles;
        public ComputeBuffer ChunkTriangleCount;
        public ComputeBuffer IndexBufferCounter;
        public ComputeBuffer TriangleBuffer;
        public ComputeBuffer TrianglesToRenderBuffer;
        public ComputeBuffer TriangulationIndices;
        public ComputeBuffer ChunkBasePositionIndex;
        public MaterialPropertyBlock PropertyBlock;
        public int3 VoxelCounts;

        public void Dispose()
        {
            if (ArgsBuffer != null) ArgsBuffer.Dispose();
            if (ChunksToTriangulate != null) ChunksToTriangulate.Dispose();
            if (ChunkTriangleCount != null) ChunkTriangleCount.Dispose();
            if (IndexBufferCounter != null) IndexBufferCounter.Dispose();
            if (TriangleBuffer != null) TriangleBuffer.Dispose();
            if (TriangulationIndices != null) TriangulationIndices.Dispose();
            if (ChunksWithTriangles != null) ChunksWithTriangles.Dispose();
            if (TrianglesToRenderBuffer != null) TrianglesToRenderBuffer.Dispose();
            if (ChunkBasePositionIndex != null) ChunkBasePositionIndex.Dispose();
            if (TrianglePositionCountBuffer != null) TrianglePositionCountBuffer.Dispose();
        }
    }
} 