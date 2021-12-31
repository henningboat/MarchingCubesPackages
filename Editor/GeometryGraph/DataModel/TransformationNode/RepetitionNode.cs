using System;
using System.Linq;
using Editor.GeometryGraph.Conversion;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.PositionModifications;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Editor.GeometryGraph.DataModel.TransformationNode
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
            GeometryInstructionListBuilder context, GeometryStackData stackData)
        {
            return new PositionModificationInstruction(TerrainTransformationType.Repetition,
                context.CurrentCombinerDepth, context.CurrentCombiner,
                _periodInput.ResolvePropertyInput(context, GeometryPropertyType.Float3));
        }
    }


    public class PositionModificationInstruction : GeometryGraphInstruction
    {
        private GeometryGraphProperty[] _properties;
        private GeometryGraphProperty _transformation;
        private CombinerState _combiner;
        private TerrainTransformationType _type;

        public PositionModificationInstruction(TerrainTransformationType type, int depth, CombinerState combiner, params GeometryGraphProperty[] properties) : base(depth)
        {
            _type = type;
            _combiner = combiner;
            _properties = properties;
        }


        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.PositionModification,
                (int) _type, _properties.ToList());
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

        public virtual void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData)
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
            GeometryInstructionListBuilder context, GeometryStackData stackData);
    }
}