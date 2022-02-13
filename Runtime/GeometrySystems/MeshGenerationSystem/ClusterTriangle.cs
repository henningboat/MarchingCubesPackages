using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public readonly struct ClusterTriangle
    {
        public readonly uint Value;

        public override string ToString()
        {
            return $"{PositionInCluster}  Cube:{RawTriangleIndex}";
        }

        public ClusterTriangle(uint value)
        {
            this.Value = value;
        }

        private const uint PositionPartBitmask = 0b111111111111111111;
        public int3 PositionInCluster
        {
            get
            {
                uint index = Value & PositionPartBitmask;
                int3 positionInCluster =
                    Runtime.DistanceFieldGeneration.Utils.IndexToPositionWS((int) index, Constants.clusterLength);
                return positionInCluster;
            }
        }

        public int CubeIndex => (int)RawTriangleIndex / 5;
        public int OffsetInCube => (int)RawTriangleIndex % 5;
        
        public uint RawTriangleIndex
        {
            get
            {
                uint index = Value >> 18;
                return index;
            }
        }
    }
}