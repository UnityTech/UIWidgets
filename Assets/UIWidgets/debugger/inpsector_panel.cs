using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIWidgets.debugger
{
    public class InspectorPanel
    {
        public readonly WidgetTreeType treeType;
        private const float splitterHeight = 8;
        private readonly InspectorTreeView m_TreeView;
        private readonly InspectorTreeView m_DetailTreeView;
        private readonly InspectorService m_InspectorService;

        private readonly EditorWindow m_Window;
        private readonly string m_GroupName;
        private Vector2 m_PropertyScrollPos = new Vector2(0, 0);
        private bool m_NeedSelectionUpdate = true;
        private bool m_NeedDetailUpdate = true;
        private List<DiagnosticsNode> m_Properties;
        private InspectorInstanceRef m_SelectedNodeRef;
        private bool m_VisibleToUser;
        private DateTime m_LastPropertyRefresh = DateTime.MinValue;
        private float m_SplitOffset = -1;


        public InspectorPanel(EditorWindow window, WidgetTreeType treeType, InspectorService inspectorService, 
            float? splitOffset = null)
        {
            this.m_Window = window;
            this.treeType = treeType;
            this.m_InspectorService = inspectorService;
            this.m_InspectorService.selectionChanged += handleSelectionChanged;
            m_TreeView = new InspectorTreeView(new TreeViewState());
            m_TreeView.onNodeSelectionChanged += OnNodeSelectionChanged;
            m_TreeView.Reload();
            if (treeType == WidgetTreeType.Widget)
            {
                m_DetailTreeView = new InspectorTreeView(new TreeViewState());
                m_DetailTreeView.Reload();
            }

            m_GroupName = InspectorObjectGroupManager.Instance.nextGroupName("inspector");
            m_SplitOffset = splitOffset ?? window.position.height / 2;
        }

        public string title
        {
            get { return treeType == WidgetTreeType.Widget ? "Widgets" : "Render Tree"; }
        }

        public bool visibleToUser
        {
            get { return m_VisibleToUser; }
            set
            {
                if (m_VisibleToUser == value)
                {
                    return;
                }

                m_VisibleToUser = value;
                if (value)
                {
                    m_NeedSelectionUpdate = true;
                    m_NeedDetailUpdate = true;
                }
            }
        }

        public PanelState PanelState
        {
            get { return new PanelState(){splitOffset = m_SplitOffset, treeType = treeType}; }
        }

        public void Close()
        {
            this.m_InspectorService.selectionChanged -= handleSelectionChanged;
            if (m_InspectorService != null)
            {
                m_InspectorService.disposeGroup(m_GroupName);
            }

            // todo
        }

        public void OnGUI()
        {
            if (Event.current.type != EventType.Layout)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                var x = lastRect.height;
            }
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
          
            // splitter
            m_SplitOffset = Math.Max(0, m_SplitOffset);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(m_SplitOffset));
            m_TreeView.OnGUI(rect);
            GUILayout.Box("",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(splitterHeight));
            var splitterRect = GUILayoutUtility.GetLastRect();
            this.splitGUI(splitterRect);

            if (m_DetailTreeView != null)
            {
                var rect2 = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_DetailTreeView.OnGUI(rect2);
            }

            if (m_Properties != null)
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_PropertyScrollPos = EditorGUILayout.BeginScrollView(m_PropertyScrollPos);
                foreach (var property in m_Properties)
                {
                    if (property.isColorProperty)
                    {
                        var properties = property.valuePropertiesJson;
                        int alpha = Util.GetIntProperty(properties, "alpha");
                        int red = Util.GetIntProperty(properties, "red");
                        int green = Util.GetIntProperty(properties, "green");

                        int blue = Util.GetIntProperty(properties, "blue");
                        var color = new UnityEngine.Color(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
                        EditorGUILayout.ColorField(property.name, color, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        EditorGUILayout.TextField(property.name, property.description);
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                    if (splitterRect.yMax > m_Window.position.height)
                    {
                        m_SplitOffset -= splitterRect.yMax - m_Window.position.height;
                        m_Window.Repaint();
                    }  
            }
            
        }


        public void LoadTree()
        {
            var node = treeType == WidgetTreeType.Widget
                ? m_InspectorService.getRootWidgetSummaryTree(m_GroupName)
                : m_InspectorService.getRootRenderObject(m_GroupName);
            m_TreeView.node = node;
        }

        public void Update()
        {
            if (!m_VisibleToUser)
            {
                return;
            }

            if (m_TreeView.node == null)
            {
                LoadTree();
            }

            updateSelection();
            updateDetailTree();

            if (treeType == WidgetTreeType.Render &&
                DateTime.Now - m_LastPropertyRefresh > TimeSpan.FromMilliseconds(200))
            {
                m_LastPropertyRefresh = DateTime.Now;
                m_Properties = m_SelectedNodeRef == null
                    ? new List<DiagnosticsNode>()
                    : m_InspectorService.getProperties(m_SelectedNodeRef, m_GroupName);
                m_Properties = m_Properties.Where((p) => p.level != DiagnosticLevel.hidden).ToList();

                m_Window.Repaint();
            }
        }


        private void OnNodeSelectionChanged(DiagnosticsNode node)
        {
            m_SelectedNodeRef = node == null ? null : node.diagnosticRef;
            m_InspectorService.setSelection(node == null ? null : node.valueRef, m_GroupName);
            m_NeedDetailUpdate = m_DetailTreeView != null;
        }

        private void handleSelectionChanged()
        {
            m_NeedSelectionUpdate = true;
            m_NeedDetailUpdate = true;
        }

        private void updateSelection()
        {
            if (!m_NeedSelectionUpdate)
            {
                return;
            }

            m_NeedSelectionUpdate = false;
            var diagnosticsNode = m_InspectorService.getSelection(m_TreeView.selectedNode, treeType, true, m_GroupName);
            m_SelectedNodeRef = diagnosticsNode == null ? null : diagnosticsNode.diagnosticRef;

            if (diagnosticsNode != null)
            {
                var item = m_TreeView.getTreeItemByValueRef(diagnosticsNode.valueRef);
                if (item == null)
                {
                    LoadTree();
                    item = m_TreeView.getTreeItemByValueRef(diagnosticsNode.valueRef);
                }

                m_TreeView.CollapseAll();
                if (item != null)
                {
                    m_TreeView.SetSelection(new List<int> {item.id}, TreeViewSelectionOptions.RevealAndFrame);
                }
                else
                {
                    m_TreeView.SetSelection(new List<int>());
                }
                m_TreeView.Repaint();
            }
        }

        private void updateDetailTree()
        {
            D.assert(!m_NeedSelectionUpdate);
            if (!m_NeedDetailUpdate)
            {
                return;
            }

            if (m_DetailTreeView == null)
            {
                return;
            }

            m_NeedDetailUpdate = false;
            if (m_SelectedNodeRef == null)
            {
                m_DetailTreeView.node = null;
            }
            else
            {
                m_DetailTreeView.node = m_InspectorService.getDetailsSubtree(m_SelectedNodeRef, m_GroupName);
            }

            m_DetailTreeView.ExpandAll();
        }

        private void splitGUI(Rect splitterRect)
        {
            var id = GUIUtility.GetControlID("inpectorPannelSplitter".GetHashCode(), FocusType.Passive);
            switch (Event.current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        m_SplitOffset += Event.current.delta.y;
                        m_Window.Repaint();
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }

                    break;
                case EventType.Repaint:
                    EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical, id);
                    break;
            }
        }
    }

    [Serializable]
    public class PanelState
    {
        public WidgetTreeType treeType;
        public float splitOffset;
    }
}