using System.Collections.Generic;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    /// <summary>
    /// Just a container for a hash. Right now I use "proper" 128bit hashes, but
    /// it's probably fine to switch that to 64bit or maybe even 32bit
    /// </summary>
    public struct GeometryInstructionHash
    {
        private Hash128 _hash;

        private sealed class HashEqualityComparer : IEqualityComparer<GeometryInstructionHash>
        {
            public bool Equals(GeometryInstructionHash x, GeometryInstructionHash y)
            {
                return x._hash.Equals(y._hash);
            }

            public int GetHashCode(GeometryInstructionHash obj)
            {
                return obj._hash.GetHashCode();
            }
        }

        public static IEqualityComparer<GeometryInstructionHash> HashComparer { get; } = new HashEqualityComparer();


        public bool Equals(GeometryInstructionHash other)
        {
            return _hash.Equals(other._hash);
        }

        public override bool Equals(object obj)
        {
            return obj is GeometryInstructionHash other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hash.GetHashCode();
        }
        public void Append<T>(ref T data) where T : unmanaged
        {
            _hash.Append(ref data);
        }
        public void Append(int data)
        {
            _hash.Append(ref data);
        }
    }
}