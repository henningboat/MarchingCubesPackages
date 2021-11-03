using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace henningboat.CubeMarching.Rendering
{
    public static class MeshGeneratorBuilder
    {
        private const int ClusterVolume = ClusterLength * ClusterLength * ClusterLength;
        private const int MaxTrianglesPerCell = 5;
        private const int VerticesPerTriangle = 3;
        private const int ClusterEntityVertexCount = ClusterVolume * MaxTrianglesPerCell * VerticesPerTriangle;
        private const int ClusterLength = 64;

        public const MeshUpdateFlags MeshUpdateFlagsNone =
            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;

        private static readonly VertexAttributeDescriptor[] ClusterMeshTerrainDescriptors =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new(VertexAttribute.Normal,VertexAttributeFormat.Float32,3),
            new(VertexAttribute.Color,VertexAttributeFormat.UNorm8,4),
        };

        private struct VertexData
        {
            public readonly float4 vertex;
            public readonly float3 normal;
            public readonly int materialData;
        }

        public static CClusterMesh GenerateClusterMesh()
        {
            var clusterMesh = new Mesh {name = "ClusterMesh", hideFlags = HideFlags.HideAndDontSave};
            clusterMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
            clusterMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

            clusterMesh.SetVertexBufferParams(ClusterEntityVertexCount, ClusterMeshTerrainDescriptors);
            clusterMesh.SetIndexBufferParams(ClusterEntityVertexCount, IndexFormat.UInt32);

            clusterMesh.bounds = new Bounds {min = Vector3.zero, max = Vector3.one * ClusterLength};
            clusterMesh.UploadMeshData(true);
            clusterMesh.SetSubMesh(0, new SubMeshDescriptor(0, 3), MeshUpdateFlagsNone);

            return new CClusterMesh {mesh = clusterMesh};
        }
    }
}