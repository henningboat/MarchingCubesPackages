using System.Collections.Generic;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.DistanceFieldGeneration
{
    /// <summary>
    /// Just a container for a hash. Right now I use "proper" 128bit hashes, but
    /// it's probably fine to switch that to 64bit or maybe even 32bit
    /// </summary>
    public readonly struct GeometryInstructionHash
    {
        public readonly Hash128 Hash;

        private sealed class HashEqualityComparer : IEqualityComparer<GeometryInstructionHash>
        {
            public bool Equals(GeometryInstructionHash x, GeometryInstructionHash y)
            {
                return x.Hash.Equals(y.Hash);
            }

            public int GetHashCode(GeometryInstructionHash obj)
            {
                return obj.Hash.GetHashCode();
            }
        }

        public static IEqualityComparer<GeometryInstructionHash> HashComparer { get; } = new HashEqualityComparer();


        public bool Equals(GeometryInstructionHash other)
        {
            return Hash.Equals(other.Hash);
        }

        public override bool Equals(object obj)
        {
            return obj is GeometryInstructionHash other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public GeometryInstructionHash(Hash128 hash)
        {
            Hash = hash;
        }
    }
}