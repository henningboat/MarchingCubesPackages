using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.CubeMarching.GeometryGraph.Editor.GraphElements
{
    public class PrintResultPart : BaseModelUIPart
    {
        public static readonly string ussClassName = "print-result-part";

        public static PrintResultPart Create(string name, IGraphElementModel model, IModelUI modelUI, string parentClassName)
        {
            if (model is GraphResult)
            {
                return new PrintResultPart(name, model, modelUI, parentClassName);
            }

            return null;
        }

        private VisualElement m_Root;

        public Button Button { get; private set; }
        public override VisualElement Root => m_Root;

        protected PrintResultPart(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName)
        {
        }

        private void OnPrintResult()
        {
        }

        protected override void BuildPartUI(VisualElement container)
        {
            m_Root = new VisualElement {name = PartName};
            m_Root.AddToClassList(ussClassName);
            m_Root.AddToClassList(m_ParentClassName.WithUssElement(PartName));

            Button = new Button() {text = "Print Result"};
            Button.clicked += OnPrintResult;
            m_Root.Add(Button);

            container.Add(m_Root);
        }

        protected override void UpdatePartFromModel()
        {
        }
    }
}