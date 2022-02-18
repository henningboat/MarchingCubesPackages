using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public readonly struct ClusterTriangle
    {
        public readonly uint Value;

        public override string ToString()
        {
            return $"{PositionInCluster} ({positionIndex})  Cube:{RawTriangleIndex}";
        }

        public ClusterTriangle(uint value)
        {
            Value = value;
        }

        private const uint PositionPartBitmask = 0b111111111111111111;

        public int3 PositionInCluster
        {
            get
            {
                var index = Value & PositionPartBitmask;
                var positionInCluster =
                    Runtime.DistanceFieldGeneration.Utils.IndexToPositionWS((int) index, Constants.clusterLength);
                return positionInCluster;
            }
        }

        public uint positionIndex => Value & PositionPartBitmask;

        public int CubeIndex => (int) RawTriangleIndex / 5;
        public int OffsetInCube => (int) RawTriangleIndex % 5;

        public uint RawTriangleIndex
        {
            get
            {
                var index = Value >> 18;
                return index;
            }
        }
    }
}