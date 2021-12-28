using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph
{
    internal class GeometryGraphToolbarProvider : IToolbarProvider
    {
        public bool ShowButton(string buttonName)
        {
            return true;
        }
    }
}