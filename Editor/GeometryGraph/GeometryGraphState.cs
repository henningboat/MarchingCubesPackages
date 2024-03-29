﻿using Editor.GeometryGraph.DataModel.GeometryNodes;
using Editor.GeometryGraph.GraphElements.Commands;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace Editor.GeometryGraph
{
    public class GeometryGraphState : GraphToolState
    {
        public GeometryGraphState(Hash128 graphViewEditorWindowGUID, Preferences preferences)
            : base(graphViewEditorWindowGUID, preferences)
        {
            this.SetInitialSearcherSize(SearcherService.Usage.k_CreateNode, new Vector2(425, 400), 2.0f);
        }

        public override void RegisterCommandHandlers(Dispatcher dispatcher)
        {
            base.RegisterCommandHandlers(dispatcher);

            if (!(dispatcher is CommandDispatcher commandDispatcher)) return;

            commandDispatcher.RegisterCommandHandler<SetNumberOfInputPortCommand>(SetNumberOfInputPortCommand
                .DefaultCommandHandler);
            commandDispatcher.RegisterCommandHandler<SetGeometryLayerCommand>(SetGeometryLayerCommand.DefaultHandler);
            commandDispatcher.RegisterCommandPreDispatchCallback(command =>
            {
                if (command is not MoveElementsCommand) ScheduleSaveAsset();
            });
        }

        private static bool sNeedsSaveAsset;

        private void ScheduleSaveAsset()
        {
            sNeedsSaveAsset = true;
            EditorApplication.delayCall += () =>
            {
                if (sNeedsSaveAsset) sNeedsSaveAsset = false;
            };
        }
    }
}