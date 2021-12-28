using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using henningboat.CubeMarching;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.PrimitiveBehaviours;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Port = UnityEditor.Experimental.GraphView.Port;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    [Serializable]
    public class CombinerNode : GeometryCombinerNode, IMultipleExecutionInputs
    {
        public IPortModel BlendModeInput { get; set; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }
        public IPortModel BlendFactor { get; set; }

        public override void Resolve(GeometryInstructionListBuilder context, GeometryStackData stackData)
        {
            var blendFactorProperty = BlendFactor.ResolvePropertyInput(context, GeometryPropertyType.Float);
            context.BeginWriteCombiner(new CombinerState(EvaluateCombinerOperation(), blendFactorProperty));
            GeometryInputA.ResolveGeometryInput(context, stackData);
            GeometryInputB.ResolveGeometryInput(context, stackData);

            context.FinishWritingCombiner();
        }

        private CombinerOperation EvaluateCombinerOperation()
        {
            var combinerOperation =
                (CombinerOperation) ((EnumValueReference) BlendModeInput.EmbeddedValue.ObjectValue).Value;
            return combinerOperation;
        }

        public override string Title
        {
            get => Enum.GetName(typeof(CombinerOperation), EvaluateCombinerOperation());
            set { }
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            BlendModeInput = this.AddDataInputPort<CombinerOperation>("Blend Mode", nameof(BlendModeInput));

            GeometryInputA = AddExecutionInput(nameof(GeometryInputA));
            GeometryInputB = AddExecutionInput(nameof(GeometryInputB));

            BlendFactor = this.AddDataInputPort<float>("BlendFactor", nameof(BlendFactor));
        }
    }

    public interface IMultipleExecutionInputs
    {
    }
}