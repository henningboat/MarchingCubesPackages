using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public class NewShapeProxy : GeometryInstance
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