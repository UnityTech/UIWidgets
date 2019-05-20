#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class WidgetsInpsectorWindow : EditorWindow {

        const float debugPaintToggleGroupWidth = 120;
        
        const float debugPaintToggleGroupHeight = 100;
        
        InspectorService m_InspectorService;
        bool m_ShowInspect;
        
        [SerializeField]
        bool m_DebugPaint;
        
        [SerializeField]
        bool m_DebugPaintSize;
        
        [SerializeField]
        bool m_DebugPaintBaseline;
        
        [SerializeField]
        bool m_DebugPaintPointer;
        
        [SerializeField]
        bool m_DebugPaintLayer;

        bool m_ShowDebugPaintToggles;

        GUIStyle m_MessageStyle;
        readonly List<InspectorPanel> m_Panels = new List<InspectorPanel>();

        Rect m_DebugPaintTogglesRect;
        int m_PanelIndex = 0;
        [SerializeField] List<PanelState> m_PanelStates = new List<PanelState>();

        List<Action> m_UpdateActions = new List<Action>();
        
        [MenuItem("Window/UIWidgets/Inspector")]
        public static void Init() {
            WidgetsInpsectorWindow window =
                (WidgetsInpsectorWindow) GetWindow(typeof(WidgetsInpsectorWindow));
            window.Show();
        }

        void OnEnable() {
            this.titleContent = new GUIContent("UIWidgets Inspector");
        }

        void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            this.DoSelectDropDown();
            bool needDebugPaintUpdate = false;
            
            if (this.m_InspectorService != null && this.m_InspectorService.debugEnabled) {
                if (GUILayout.Button("Refersh", EditorStyles.toolbarButton)) {
                    foreach (var panel in this.m_Panels) {
                        panel.MarkNeedReload();
                    }
                }
                
                EditorGUI.BeginChangeCheck();
                var newShowInspect = GUILayout.Toggle(this.m_ShowInspect, new GUIContent("Inspect Element"),
                    EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck()) {
                    this.m_InspectorService.setShowInspect(newShowInspect);
                }

                var style = (GUIStyle) "GV Gizmo DropDown";
                Rect r = GUILayoutUtility.GetRect(new GUIContent("Debug Paint"), style);
                Rect rightRect = new Rect(r.xMax - style.border.right, r.y, style.border.right, r.height);
                if (EditorGUI.DropdownButton(rightRect, GUIContent.none, FocusType.Passive, GUIStyle.none))
                {
                    this.ScheduleUpdateAction(() => {
                        this.m_ShowDebugPaintToggles = !this.m_ShowDebugPaintToggles;
                        this.Repaint();
                    });
                }

                if (Event.current.type == EventType.Repaint) {
                    this.m_DebugPaintTogglesRect = new Rect(r.xMax - debugPaintToggleGroupWidth, r.yMax + 2, 
                        debugPaintToggleGroupWidth, debugPaintToggleGroupHeight);
                }

                EditorGUI.BeginChangeCheck();
                this.m_DebugPaint = GUI.Toggle(r, this.m_DebugPaint, new GUIContent("Debug Paint"), style);
                if (EditorGUI.EndChangeCheck()) {
                    if (this.m_DebugPaint) {
                        if (!this.m_DebugPaintSize && !this.m_DebugPaintLayer
                                                   && !this.m_DebugPaintPointer && !this.m_DebugPaintBaseline) {
                            this.m_DebugPaintSize = true;
                        }
                    }
                    needDebugPaintUpdate = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (this.m_InspectorService != null && this.m_InspectorService .debugEnabled) {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
                this.m_Panels.Each((pannel, index) => {
                    if (GUILayout.Toggle(this.m_PanelIndex == index, pannel.title, EditorStyles.toolbarButton,
                        GUILayout.ExpandWidth(false), GUILayout.Width(100))) {
                        this.m_PanelIndex = index;
                    }
                });
                EditorGUILayout.EndHorizontal();

                bool shouldHandleGUI = true;
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) {
                    if (this.m_ShowDebugPaintToggles && this.m_DebugPaintTogglesRect.Contains(Event.current.mousePosition)) {
                        shouldHandleGUI = false;
                    }
                }

                if (shouldHandleGUI) {
                    this.m_Panels[this.m_PanelIndex].OnGUI();
                }
            } else if (this.m_InspectorService != null) { // debug not enabled
                if (this.m_MessageStyle == null) {
                    this.m_MessageStyle = new GUIStyle(GUI.skin.label);
                    this.m_MessageStyle.fontSize = 16;
                    this.m_MessageStyle.alignment = TextAnchor.MiddleCenter;
                    this.m_MessageStyle.padding = new RectOffset(20, 20, 40, 0);
                }
                GUILayout.Label("You're not in UIWidgets Debug Mode.\nPlease define UIWidgets_DEBUG " +
                                "symbols at \"Player Settings => Scripting Define Symbols\".",
                    this.m_MessageStyle, GUILayout.ExpandWidth(true));
            }
            
           if (this.m_ShowDebugPaintToggles) {
               this.DebugPaintToggles(ref needDebugPaintUpdate);
           }

           if (needDebugPaintUpdate) {
               D.setDebugPaint(
                   debugPaintSizeEnabled: this.m_DebugPaint && this.m_DebugPaintSize, 
                   debugPaintBaselinesEnabled: this.m_DebugPaint && this.m_DebugPaintBaseline,
                   debugPaintPointersEnabled: this.m_DebugPaint && this.m_DebugPaintPointer,
                   debugPaintLayerBordersEnabled: this.m_DebugPaint && this.m_DebugPaintLayer,
                   debugRepaintRainbowEnabled: this.m_DebugPaint && this.m_DebugPaintLayer
                   );
           }
        }

        void DebugPaintToggles(ref bool needUpdate) {
            GUILayout.BeginArea(this.m_DebugPaintTogglesRect, GUI.skin.box);
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(4);
            this.m_DebugPaintSize = GUILayout.Toggle(this.m_DebugPaintSize, new GUIContent("Paint Size"));
            this.m_DebugPaintBaseline = GUILayout.Toggle(this.m_DebugPaintBaseline, new GUIContent("Paint Baseline"));
            this.m_DebugPaintPointer = GUILayout.Toggle(this.m_DebugPaintPointer, new GUIContent("Paint Pointer"));
            this.m_DebugPaintLayer  = GUILayout.Toggle(this.m_DebugPaintLayer, new GUIContent("Paint Layer"));
            if (EditorGUI.EndChangeCheck()) {
                needUpdate = true;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (Event.current.type == EventType.MouseDown && 
                !this.m_DebugPaintTogglesRect.Contains(Event.current.mousePosition)) {
                this.ScheduleUpdateAction(() => {
                    this.m_ShowDebugPaintToggles = false;
                    this.Repaint();
                });
            }
        }
        
        void DoSelectDropDown() {
            var currentWindow = this.m_InspectorService == null ? null : this.m_InspectorService.window;
            if (currentWindow != null && !currentWindow.alive) {
                currentWindow = null;
            }
            
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
            this.m_ShowDebugPaintToggles = false;
        }

        void ScheduleUpdateAction(Action action) {
            this.m_UpdateActions.Add(action);
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

            while (this.m_UpdateActions.Count > 0) {
                this.m_UpdateActions[0]();
                this.m_UpdateActions.RemoveAt(0);
            }
        }


        void OnDestroy() {
            this.closeInspect();
        }
    }
}
#endif