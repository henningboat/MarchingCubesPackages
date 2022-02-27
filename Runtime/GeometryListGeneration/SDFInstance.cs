using System.Collections.Generic;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.GeometryListGeneration
{
    public class SDFInstance : GeometryInstance
    {
        [SerializeField] private Texture3D _sdf;
        
        [SerializeField] private GeometryInstructionList _instructionList;

        public override GeometryInstructionList GeometryInstructionList => _instructionList;

        private void Awake()
        {
            BuildInstructionList();
        }

        private void OnValidate()
        {
            BuildInstructionList();
        }

        private void BuildInstructionList()
        {
            using (var builder = new GeometryInstructionListBuilder())
            {
                var properties = new List<GeometryGraphProperty>();
                properties.Add(builder.CreateProperty(1.0f));
                properties.Add(builder.CreateProperty(0.0f));
                properties.Add(builder.CreateProperty(10.0f));
                builder.WriteInstruction(
                    GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType.SDF2D,
                        properties), _sdf);

                _instructionList = builder.GetGeometryGraphData();
            }
        }
    }
}