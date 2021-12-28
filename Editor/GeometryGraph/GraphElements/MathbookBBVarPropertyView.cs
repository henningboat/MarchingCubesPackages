using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.GraphElements
{
    public class GeometryGraphBBVarPropertyView : BlackboardVariablePropertyView
    {
        protected override void BuildRows()
        {
            AddInitializationField();
            AddTooltipField();
        }

    }
}
