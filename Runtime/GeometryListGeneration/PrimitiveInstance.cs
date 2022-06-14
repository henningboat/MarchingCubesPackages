using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    public class PrimitiveInstance : GeometryInstance
    {
        [SerializeField] private ShapeType _shapeType;
        [SerializeField] private List<float> _properties;
        
        [SerializeField] private GeometryInstructionList instructionList;

        public override GeometryInstructionList GeometryInstructionList => instructionList;

#if UNITY_EDITOR
        public void Initialize(GeometryInstructionList geometryInstructionList)
        {
            instructionList = geometryInstructionList;
        }
#endif
    }
}