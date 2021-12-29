using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    public class PrimitiveInstance : GeometryInstance
    {
        [SerializeField] private ShapeType _shapeType;
        [SerializeField] private GeometryInstructionList instructionList;

#if UNITY_EDITOR
        public void Initialize(GeometryInstructionList geometryInstructionList)
        {
            instructionList = geometryInstructionList;
        }
        #endif
        
        public override GeometryInstructionList GeometryInstructionList => instructionList;
    }
}