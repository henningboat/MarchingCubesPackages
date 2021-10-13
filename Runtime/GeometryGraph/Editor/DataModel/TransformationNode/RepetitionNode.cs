using System.Linq;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
     [UsedImplicitly]
     public class RepetitionNode : PositionModificationNode
     {
         private IPortModel _periodInput;
    
         protected override void OnDefineNode()
         {
             base.OnDefineNode();
             _periodInput = this.AddDataInputPort("", nameof(_periodInput), defaultValue: Vector3.one * 8);
         }

         protected override PositionModificationInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context, GeometryStackData stackData)
         {
             return new(TerrainTransformationType.Repetition, context.CurrentCombinerDepth, context.CurrentCombiner, stackData.Transformation,
                 _periodInput.ResolvePropertyInput(context, GeometryPropertyType.Float3));
         }
     }

     
     public class PositionModificationInstruction : GeometryGraphInstruction
     {
         private GeometryGraphProperty[] _properties;
         private GeometryGraphProperty _transformation;
         private CombinerInstruction _combiner;
         private TerrainTransformationType _type;

         public PositionModificationInstruction(TerrainTransformationType type, int depth, CombinerInstruction combiner, GeometryGraphProperty transformation,  params GeometryGraphProperty[] properties) : base(depth)
         {
             _type = type;
             _combiner = combiner;
             _transformation = transformation;
             _properties = properties;
         }


         public override GeometryInstruction GetInstruction()
         {
             return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.PositionModification, (int) _type, Depth, _combiner.GetCombinerSetting(), _transformation, _properties.ToList(),
                 null);
         }
     }

     public abstract class PositionModificationNode:NodeModel,IGeometryNode
     {
         private IPortModel _geometryIn;
         private IPortModel _geometryOut;

         protected override void OnDefineNode()
         {
             _geometryIn = this.AddExecutionInputPort("", nameof(_geometryIn));
             _geometryOut = this.AddExecutionOutputPort("", nameof(_geometryOut));
         }

         public void Resolve(GeometryGraphResolverContext context, GeometryStackData stackData)
         {
             context.WritePositionModificationModifier(GetDistanceModifierInstruction(context,stackData));

             var zeroMatrixInstruction = context.GetOrCreateProperty(SerializableGUID.Generate(),
                 new GeometryGraphConstantProperty(Matrix4x4.identity, context, GeometryPropertyType.Float4X4, "Identity Transformation"));
             stackData.Transformation = zeroMatrixInstruction;
             
             _geometryIn.ResolveGeometryInput(context, stackData);
         }

         protected abstract PositionModificationInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context, GeometryStackData stackData);
     }
}