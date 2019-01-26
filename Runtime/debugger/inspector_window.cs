#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class WidgetsInpsectorWindow : EditorWindow {
        InspectorService m_InspectorService;
        bool m_ShowInspect;
        readonly List<InspectorPanel> m_Panels = new List<InspectorPanel>();
        int m_PanelIndex = 0;
        [SerializeField] List<PanelState> m_PanelStates = new List<PanelState>();

        [MenuItem("Window/Analysis/UIWidgets Inspector")]
        public static void Init() {
            WidgetsInpsectorWindow window =
                (WidgetsInpsectorWindow) GetWindow(typeof(WidgetsInpsectorWindow));
            window.Show();
        }

        void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            this.DoSelectDropDown();
            if (this.m_InspectorService != null) {
                EditorGUI.BeginChangeCheck();
                var newShowInspect = GUILayout.Toggle(this.m_ShowInspect, new GUIContent("Inspect Element"),
                    EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck()) {
                    this.m_InspectorService.setShowInspect(newShowInspect);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (this.m_InspectorService != null) {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
                this.m_Panels.Each((pannel, index) => {
                    if (GUILayout.Toggle(this.m_PanelIndex == index, pannel.title, EditorStyles.toolbarButton,
                        GUILayout.ExpandWidth(false), GUILayout.Width(100))) {
                        this.m_PanelIndex = index;
                    }
                });
                EditorGUILayout.EndHorizontal();

                this.m_Panels[this.m_PanelIndex].OnGUI();
            }
        }

        void DoSelectDropDown() {
            var currentWindow = this.m_InspectorService == null ? null : this.m_InspectorService.window;
            var selectTitle = currentWindow != null ? currentWindow.titleContent : new GUIContent("<Please Select>");
            if (GUILayout.Button(selectTitle, EditorStyles.toolbarDropDown)) {
                var windows = new List<WindowAdapter>(WindowAdapter.windowAdapters.Where(w => {
                    return w.withBindingFunc(() => WidgetsBinding.instance.renderViewElement != null);
                }));
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                    EditorStyles.toolbarDropDown);
                var menuPos = EditorGUI.IndentedRect(rect);
                menuPos.y += EditorGUIUtility.singleLineHeight / 2;

                int selectedIndex = 0;
                var labels = new GUIContent[windows.Count + 1];
                labels[0] = new GUIContent("none");
                for (int i = 0; i < windows.Count; i++) {
                    labels[i + 1] = windows[i].titleContent;
                    if (windows[i] == currentWindow) {
                        selectedIndex = i + 1;
                    }
                }

                EditorUtility.DisplayCustomMenu(menuPos, labels, selectedIndex, (data, options, selected) => {
                    if (selected > 0) {
                        var selectedWindow = windows[selected - 1];
                        if (selectedWindow != currentWindow) {
                            this.inspect(selectedWindow);
                        }
                    }
                    else {
                        if (this.m_InspectorService != null) {
                            this.closeInspect();
                        }
                    }
                }, null);
            }
        }

        void inspect(WindowAdapter window) {
            if (this.m_InspectorService != null) // stop previous inspect
            {
                this.closeInspect();
            }

            this.m_InspectorService = new InspectorService(window);
            this.m_PanelIndex = 0;

            var state = this.m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Widget);
            this.m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Widget, this.m_InspectorService,
                state == null ? (float?) null : state.splitOffset));

            state = this.m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Render);
            this.m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Render, this.m_InspectorService,
                state == null ? (float?) null : state.splitOffset));
        }

        void closeInspect() {
            if (this.m_InspectorService == null) {
                return;
            }

            this.m_InspectorService.close();
            this.m_InspectorService = null;
            foreach (var panel in this.m_Panels) {
                panel.Close();
            }

            this.m_Panels.Clear();
            this.m_ShowInspect = false;
        }

        void Update() {
            if (this.m_InspectorService != null && !this.m_InspectorService.active) {
                this.closeInspect();
                this.Repaint();
            }

            bool showInspect = false;
            if (this.m_InspectorService != null) {
                showInspect = this.m_InspectorService.getShowInspect();
            }

            if (showInspect != this.m_ShowInspect) {
                this.Repaint();
            }

            this.m_ShowInspect = showInspect;

            for (int i = 0; i < this.m_Panels.Count; i++) {
                this.m_Panels[i].visibleToUser = this.m_PanelIndex == i;
                this.m_Panels[i].Update();
            }

            if (this.m_Panels.Count > 0) {
                this.m_PanelStates = this.m_Panels.Select(p => p.PanelState).ToList();
            }
        }


        void OnDestroy() {
            this.closeInspect();
        }
    }
}
#endif