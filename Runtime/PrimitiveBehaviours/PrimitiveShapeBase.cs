using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometrySystems.GenerationGraphSystem;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Collections;
using UnityEngine;

namespace henningboat.CubeMarching.PrimitiveBehaviours
{
    public abstract class PrimitiveShapeBase : MonoBehaviour
    {
        protected RuntimeGeometryGraphResolverContext PrimitiveBuilder;

        private void BuildPrimitive()
        {
            PrimitiveBuilder = new RuntimeGeometryGraphResolverContext();
            PrimitiveBuilder.AddShape(GetShapeProxy());
            
            
            
            PrimitiveBuilder.Dispose();
        }

        protected abstract GeometryInstructionProxy GetShapeProxy();
    }

    public abstract class ShapeProxy:GeometryInstructionProxy
    {

        public override GeometryInstructionType GeometryInstructionType => GeometryInstructionType.Shape;

    }

    public class GeometryGraphBuilder
    {
        private GeometryGraphData _result;

        private NativeList<float> _valueBuffer;
        private NativeList<GeometryInstruction> _instructions;

        public GeometryGraphBuilder()
        {
            _valueBuffer = new NativeList<float>(Allocator.Temp);
            _instructions = new NativeList<GeometryInstruction>(Allocator.Temp);

            AddSphere();
        }

        private void AddSphere()
        {
            _instructions.Add(new GeometryInstruction()
            {
                _flags = GeometryInstructionFlags.None,
                CombinerDepth = 0,
                GeometryInstructionType = GeometryInstructionType.Shape,
                GeometryInstructionSubType = (int)ShapeType.Sphere,
                
            });
        }
    }
}
