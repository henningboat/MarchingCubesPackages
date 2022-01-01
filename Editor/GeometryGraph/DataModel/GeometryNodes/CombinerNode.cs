using System;
using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    [Serializable]
    public class CombinerNode : GeometryCombinerNode, IMultipleExecutionInputs
    {
        public IPortModel BlendModeInput { get; set; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }
        public IPortModel BlendFactor { get; set; }

        public override void Resolve(GeometryInstructionListBuilder context)
        {
            var blendFactorProperty = BlendFactor.ResolvePropertyInput(context, GeometryPropertyType.Float);
            context.BeginWriteCombiner(EvaluateCombinerOperation(), blendFactorProperty);
            GeometryInputA.ResolveGeometryInput(context);
            GeometryInputB.ResolveGeometryInput(context);

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