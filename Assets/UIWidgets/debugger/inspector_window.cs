#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using UIWidgets.editor;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.debugger
{
    public class WidgetsInpsectorWindow: EditorWindow
    {
        
        private InspectorService m_InspectorService;
        private bool m_ShowInspect;
        private readonly List<InspectorPanel> m_Panels = new List<InspectorPanel>();
        private int m_PanelIndex = 0;
        [SerializeField]
        private List<PanelState> m_PanelStates = new List<PanelState>();
        
        [MenuItem("Window/Analysis/UIWidgets Inspector")]
        public static void Init()
        {
            WidgetsInpsectorWindow window = (WidgetsInpsectorWindow)EditorWindow.GetWindow(typeof(WidgetsInpsectorWindow));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            DoSelectDropDown();
            if (m_InspectorService != null)
            {
                EditorGUI.BeginChangeCheck();
                var newShowInspect = GUILayout.Toggle(this.m_ShowInspect, new GUIContent("Inspect Element"), EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    m_InspectorService.setShowInspect(newShowInspect);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
          
            if (m_InspectorService != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
                m_Panels.Each((pannel, index) =>
                {
                    if (GUILayout.Toggle(m_PanelIndex == index, pannel.title, EditorStyles.toolbarButton,
                        GUILayout.ExpandWidth(false), GUILayout.Width(100)))
                    {
                        m_PanelIndex = index;
                    }
                });
                EditorGUILayout.EndHorizontal();

                m_Panels[m_PanelIndex].OnGUI();
            }
           
        }
        
        private void DoSelectDropDown()
        {
            var currentWindow = m_InspectorService == null ? null : m_InspectorService.window;
            var selectTitle = currentWindow != null ? currentWindow.titleContent :  new GUIContent("<Please Select>");
            if (GUILayout.Button(selectTitle, EditorStyles.toolbarDropDown))
            {
                var windows = new List<WindowAdapter>(WindowAdapter.windowAdapters.Where(w =>
                {
                    return w.WithBindingFunc(() => WidgetsBinding.instance.renderViewElement != null);
                }));
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.toolbarDropDown);
                var menuPos = EditorGUI.IndentedRect(rect);
                menuPos.y += EditorGUIUtility.singleLineHeight / 2;
                
                int selectedIndex = 0;
                var labels = new GUIContent[windows.Count + 1];
                labels[0] = new GUIContent("none");
                for (int i = 0; i < windows.Count; i++)
                {
                    labels[i + 1] = windows[i].titleContent;
                    if (windows[i] == currentWindow)
                    {
                        selectedIndex = i + 1;
                    }
                }
                EditorUtility.DisplayCustomMenu(menuPos, labels, selectedIndex, (data, options, selected) =>
                {
                    if (selected > 0)
                    {
                        var selectedWindow = windows[selected - 1];
                        if (selectedWindow != currentWindow)
                        {
                            inspect(selectedWindow);
                        }
                    }
                    else
                    {
                        if (m_InspectorService != null)
                        {
                            closeInspect();
                        }
                    }
                }, null);
            }
        }

        private void inspect(WindowAdapter window)
        {
            if (m_InspectorService != null) // stop previous inspect
            {
                closeInspect();
            }

            m_InspectorService = new InspectorService(window);
            m_PanelIndex = 0;

            var state = m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Widget);
            m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Widget, m_InspectorService, 
                state == null ? (float?)null : state.splitOffset));
            
            state = m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Render);
            m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Render, m_InspectorService,
                state == null ? (float?)null : state.splitOffset));
        }

        private void closeInspect()
        {
            if (m_InspectorService == null)
            {
                return;
            }
            m_InspectorService.close();
            m_InspectorService = null;
            foreach (var panel in m_Panels)
            {
                panel.Close();
            }
            m_Panels.Clear();
            m_ShowInspect = false;
        }

        private void Update()
        {
            if (m_InspectorService != null && !m_InspectorService.active)
            {
                closeInspect();
                Repaint();
            }

            bool showInspect = false;
            if (m_InspectorService != null)
            {
                showInspect = m_InspectorService.getShowInspect();
            }
            if (showInspect != this.m_ShowInspect)
            {
                Repaint();
            }
            m_ShowInspect = showInspect;

            for (int i = 0; i < m_Panels.Count; i++)
            {
                m_Panels[i].visibleToUser = m_PanelIndex == i;
                m_Panels[i].Update();
            }

            if (m_Panels.Count > 0)
            {
                m_PanelStates = m_Panels.Select(p => p.PanelState).ToList();
            }
        }
        
        
        void OnDestroy()
        {
            closeInspect();
        }
     }
    
}
#endif
