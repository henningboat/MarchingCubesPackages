using System;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using JetBrains.Annotations;
using Unity.Mathematics;
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

        protected override PositionModificationInstruction GetDistanceModifierInstruction(
            RuntimeGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            return new PositionModificationInstruction(TerrainTransformationType.Repetition,
                context.CurrentCombinerDepth, context.CurrentCombiner,
                stackData.Transformation,
                _periodInput.ResolvePropertyInput(context, GeometryPropertyType.Float3));
        }
    }


    public class PositionModificationInstruction : GeometryGraphInstruction
    {
        private GeometryGraphProperty[] _properties;
        private GeometryGraphProperty _transformation;
        private CombinerState _combiner;
        private TerrainTransformationType _type;

        public PositionModificationInstruction(TerrainTransformationType type, int depth, CombinerState combiner,
            GeometryGraphProperty transformation, params GeometryGraphProperty[] properties) : base(depth)
        {
            _type = type;
            _combiner = combiner;
            _transformation = transformation;
            _properties = properties;
        }


        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.PositionModification,
                (int) _type, _transformation, _properties.ToList());
        }
    }

    public abstract class PositionModificationNode : GeometryNodeModel, IGeometryNode
    {
        protected IPortModel _geometryIn;
        private IPortModel _geometryOut;

        protected override void OnDefineNode()
        {
            _geometryIn = AddExecutionInput(nameof(_geometryIn));
            _geometryOut = AddExecutionOutput(nameof(_geometryOut));
        }

        public virtual void Resolve(RuntimeGeometryGraphResolverContext context, GeometryStackData stackData)
        {
            throw new NotImplementedException();
            // context.WritePositionModificationModifier(GetDistanceModifierInstruction(context, stackData));
            //
            // var zeroMatrixInstruction = context.GetOrCreateProperty(SerializableGUID.Generate(), Matrix4x4.identity);
            // stackData.Transformation = zeroMatrixInstruction;
            //
            // _geometryIn.ResolveGeometryInput(context, stackData);
        }

        protected abstract PositionModificationInstruction GetDistanceModifierInstruction(
            RuntimeGeometryGraphResolverContext context, GeometryStackData stackData);
    }
}