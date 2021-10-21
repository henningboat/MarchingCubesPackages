using System;
using UnityEngine;

namespace henningboat.CubeMarching.Rendering
{
    [Serializable]
    public struct CClusterMesh : IEquatable<CClusterMesh>
    {
        public Mesh mesh;

        public bool Equals(CClusterMesh other)
        {
            return Equals(mesh, other.mesh);
        }

        public override bool Equals(object obj)
        {
            return obj is CClusterMesh other && Equals(other);
        }

        public override int GetHashCode()
        {
            return mesh != null ? mesh.GetHashCode() : 0;
        }
    }
}