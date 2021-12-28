using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.UIElements;

namespace Editor.GeometryGraph
{
    public class GeometryGraphOnboardingProvider : OnboardingProvider
    {
        public override VisualElement CreateOnboardingElements(CommandDispatcher store)
        {
            var template = new GraphTemplate<GeometryGraphStencil>(GeometryGraphStencil.GraphName);
            return AddNewGraphButton<GeometryGraphAsset>(template);
        }
    }
}
