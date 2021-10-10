using System;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
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