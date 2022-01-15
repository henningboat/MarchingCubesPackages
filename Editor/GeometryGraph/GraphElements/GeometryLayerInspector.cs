using System.Collections.Generic;
using Editor.GeometryGraph.DataModel.GeometryNodes;
using Editor.GeometryGraph.DataModel.MathNodes;
using Editor.GeometryGraph.GraphElements.Commands;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GeometryGraph.GraphElements
{
    internal class MathOperatorInspector : FieldsInspector
    {
        public static MathOperatorInspector Create(string name, IGraphElementModel model, IModelUI ownerElement,
            string parentClassName)
        {
            if (model is MathOperator) return new MathOperatorInspector(name, model, ownerElement, parentClassName);

            return null;
        }

        /// <inheritdoc />
        public MathOperatorInspector(string name, IGraphElementModel model, IModelUI ownerElement,
            string parentClassName)
            : base(name, model, ownerElement, parentClassName)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<BaseModelPropertyField> GetFields()
        {
            if (m_Model is MathOperator mathOperator)
                yield return new ModelPropertyField<int>(
                    m_OwnerElement.CommandDispatcher,
                    mathOperator,
                    nameof(mathOperator.InputPortCount),
                    "Number of Inputs",
                    typeof(SetNumberOfInputPortCommand));
        }
    }

    // internal class GeometryLayerInspector : NodeFieldsInspector
    // {
    //     public new static GeometryLayerInspector Create(string name, IGraphElementModel model, IModelUI ownerElement,
    //         string parentClassName)
    //     {
    //         if (model is CopyGeometryLayerNodeModel)
    //         {
    //             return new GeometryLayerInspector(name, model, ownerElement, parentClassName);
    //         }
    //
    //         return null;
    //     }
    //
    //     /// <inheritdoc />
    //     public GeometryLayerInspector(string name, IGraphElementModel model, IModelUI ownerElement,
    //         string parentClassName)
    //         : base(name, model, ownerElement, parentClassName)
    //     {
    //     }
    //
    //     /// <inheritdoc />
    //     protected override IEnumerable<BaseModelPropertyField> GetFields()
    //     {
    //         foreach (var propertyField in base.GetFields())
    //         {
    //             yield return propertyField;
    //         }
    //
    //         if (m_Model is CopyGeometryLayerNodeModel copyGeometryLayerNode)
    //         {
    //             yield return new ModelPropertyField<GeometryLayer>(
    //                 m_OwnerElement.CommandDispatcher,
    //                 copyGeometryLayerNode,
    //                 nameof(CopyGeometryLayerNodeModel.SourceLayer),
    //                 "SourceLayer",
    //                 typeof(SetGeometryLayerCommand));
    //         }
    //     }
    // }


    public class TemperatureAndTimePart : BaseModelUIPart
    {
        public static readonly string ussClassName = "ge-sample-bake-node-part";
        public static readonly string temperatureLabelName = "temperature";
        public static readonly string durationLabelName = "duration";

        public static TemperatureAndTimePart Create(string name, IGraphElementModel model,
            IModelUI modelUI, string parentClassName)
        {
            if (model is INodeModel) return new TemperatureAndTimePart(name, model, modelUI, parentClassName);

            return null;
        }

        private VisualElement TemperatureAndTimeContainer { get; set; }
        private ObjectField TemperatureLabel { get; set; }

        public override VisualElement Root => TemperatureAndTimeContainer;

        private TemperatureAndTimePart(string name, IGraphElementModel model, IModelUI ownerElement,
            string parentClassName) : base(name, model, ownerElement, parentClassName)
        {
        }

        protected override void BuildPartUI(VisualElement container)
        {
            if (!(m_Model is CopyGeometryLayerNodeModel))
                return;

            TemperatureAndTimeContainer = new VisualElement {name = PartName};
            TemperatureAndTimeContainer.AddToClassList(ussClassName);
            TemperatureAndTimeContainer.AddToClassList(
                m_ParentClassName.WithUssElement(PartName));

            TemperatureLabel = new ObjectField("GeometryLayer")
                {name = temperatureLabelName, objectType = typeof(GeometryLayerAsset)};
            TemperatureLabel.RegisterCallback<ChangeEvent<Object>>(OnLayerChange);
            TemperatureAndTimeContainer.Add(TemperatureLabel);

            container.Add(TemperatureAndTimeContainer);
        }

        protected override void PostBuildPartUI()
        {
            base.PostBuildPartUI();

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.unity.graphtools.foundation/Samples/Recipes/Editor/UI/Stylesheets/BakeNodePart.uss");

            if (stylesheet != null) TemperatureAndTimeContainer.styleSheets.Add(stylesheet);
        }

        private void OnLayerChange(ChangeEvent<Object> evt)
        {
            if (!(m_Model is CopyGeometryLayerNodeModel geometryLayerNode))
                return;

            m_OwnerElement.CommandDispatcher.Dispatch(
                new SetGeometryLayerCommand(new[] {geometryLayerNode}, evt.newValue as GeometryLayerAsset));
        }

        protected override void UpdatePartFromModel()
        {
            if (!(m_Model is CopyGeometryLayerNodeModel geometryLayerNode))
                return;

            TemperatureLabel.SetValueWithoutNotify(geometryLayerNode.SourceLayerAsset);
        }
    }
}