using System.Linq;
using Editor.GeometryGraph.DataModel.MathNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace Editor.GeometryGraph.GraphElements.Commands
{
    public class SetNumberOfInputPortCommand : ModelCommand<MathOperator, int>
    {
        private const string k_UndoStringSingular = "Change Input Count";

        public SetNumberOfInputPortCommand(int inputCount, params MathOperator[] nodes)
            : base(k_UndoStringSingular, k_UndoStringSingular, inputCount, nodes)
        {
        }

        public static void DefaultCommandHandler(GraphToolState graphToolState, SetNumberOfInputPortCommand command)
        {
            if (!command.Models.Any())
            {
                return;
            }

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.InputPortCount = command.Value;
                    nodeModel.DefineNode();
                }

                graphUpdater.MarkChanged(command.Models);
            }
        }
    }
}