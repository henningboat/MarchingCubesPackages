using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Editor.GeometryGraph.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(GeometryInstructionListBuilder context);
    }
}