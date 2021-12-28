using System;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GeometryGraph
{
    public class GeometryGraphMainToolbar : MainToolbar
    {
        public GeometryGraphMainToolbar(CommandDispatcher commandDispatcher, GraphView graphView)
            : base(commandDispatcher, graphView)
        {
            var geometryGraphWindow = graphView.Window as GeometryGraphViewWindow;
            var toggleButton = new Toggle("Preview Selected Element");
            toggleButton.RegisterValueChangedCallback(geometryGraphWindow.OnLivePreviewToggle);
            this.Add(toggleButton);
        }

        protected override void BuildOptionMenu(GenericMenu menu)
        {
            base.BuildOptionMenu(menu);

            var preferences = m_CommandDispatcher.State.Preferences;

            GUIContent CreateTextContent(string content)
            {
                // TODO: Replace by EditorGUIUtility.TrTextContent when it's made 'public'.
                return new GUIContent(content);
            }

            void MenuItem(string title, bool value, GenericMenu.MenuFunction onToggle)
                => menu.AddItem(CreateTextContent(title), value, onToggle);

            void MenuToggle(string title, BoolPref k, Action callback = null)
            {
                if (preferences != null)
                    MenuItem(title, preferences.GetBool(k), () =>
                    {
                        preferences.ToggleBool(k);
                        callback?.Invoke();
                    });
            }

            menu.AddSeparator("");
            MenuToggle("Auto Itemize Constants", BoolPref.AutoItemizeConstants);
            MenuToggle("Auto Itemize Variables", BoolPref.AutoItemizeVariables);
        }
    }
}
