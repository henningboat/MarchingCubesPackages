using Unity.Mathematics;
using UnityEngine;

namespace henningboat.CubeMarching.GeometrySystems.GeometryFieldSetup
{
    public struct GeometryChunkParameters
    {
        public byte ChunkInsideTerrain;
        public byte InnerDataMask;
        public int IndexInDistanceFieldBuffer;
        public bool HasData => InnerDataMask != 0;
        public int InstructionChangeTimeStamp;

        public Hash128 CurrentGeometryInstructionsHash;
        public bool InstructionsChangedSinceLastFrame;
        public int IndexInCluster;
        public int3 PositionWS;
    }
}