using System;

namespace Editor.GeometryGraph.DataModel.MathNodes
{
    [Serializable]
    public class CosFunction : MathFunction
    {
        public override string Title
        {
            get => "Cos";
            set { }
        }

        public CosFunction()
        {
            if (m_ParameterNames.Length == 0)
            {
                m_ParameterNames = new[] {"f"};
            }
        }
    }
}