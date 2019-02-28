#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class InspectorPanel {
        public readonly WidgetTreeType treeType;
        const float splitterHeight = 8;
        readonly InspectorTreeView m_TreeView;
        readonly InspectorTreeView m_DetailTreeView;
        readonly InspectorService m_InspectorService;

        readonly EditorWindow m_Window;
        readonly string m_GroupName;
        Vector2 m_PropertyScrollPos = new Vector2(0, 0);
        bool m_NeedSelectionUpdate = true;
        bool m_NeedDetailUpdate = true;
        List<DiagnosticsNode> m_Properties;
        InspectorInstanceRef m_SelectedNodeRef;
        bool m_VisibleToUser;
        TimeSpan m_LastPropertyRefresh = TimeSpan.Zero;
        float m_SplitOffset = -1;


        public InspectorPanel(EditorWindow window, WidgetTreeType treeType, InspectorService inspectorService,
            float? splitOffset = null) {
            this.m_Window = window;
            this.treeType = treeType;
            this.m_InspectorService = inspectorService;
            this.m_InspectorService.selectionChanged += this.handleSelectionChanged;
            this.m_TreeView = new InspectorTreeView(new TreeViewState());
            this.m_TreeView.onNodeSelectionChanged += this.OnNodeSelectionChanged;
            this.m_TreeView.Reload();
            if (treeType == WidgetTreeType.Widget) {
                this.m_DetailTreeView = new InspectorTreeView(new TreeViewState());
                this.m_DetailTreeView.Reload();
            }

            this.m_GroupName = Singleton<InspectorObjectGroupManager>.Instance.nextGroupName("inspector");
            this.m_SplitOffset = splitOffset ?? window.position.height / 2;
        }

        public string title {
            get { return this.treeType == WidgetTreeType.Widget ? "Widgets" : "Render Tree"; }
        }

        public bool visibleToUser {
            get { return this.m_VisibleToUser; }
            set {
                if (this.m_VisibleToUser == value) {
                    return;
                }

                this.m_VisibleToUser = value;
                if (value) {
                    this.m_NeedSelectionUpdate = true;
                    this.m_NeedDetailUpdate = true;
                }
            }
        }

        public PanelState PanelState {
            get { return new PanelState() {splitOffset = this.m_SplitOffset, treeType = this.treeType}; }
        }

        public void Close() {
            this.m_InspectorService.selectionChanged -= this.handleSelectionChanged;
            if (this.m_InspectorService != null) {
                this.m_InspectorService.disposeGroup(this.m_GroupName);
            }

            // todo
        }

        public void OnGUI() {
            if (Event.current.type != EventType.Layout) {
                var lastRect = GUILayoutUtility.GetLastRect();
                var x = lastRect.height;
            }

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // splitter
            this.m_SplitOffset = Mathf.Max(0, this.m_SplitOffset);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true),
                GUILayout.Height(this.m_SplitOffset));
            this.m_TreeView.OnGUI(rect);
            GUILayout.Box("",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(splitterHeight));
            var splitterRect = GUILayoutUtility.GetLastRect();
            this.splitGUI(splitterRect);

            if (this.m_DetailTreeView != null) {
                var rect2 = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                this.m_DetailTreeView.OnGUI(rect2);
            }

            if (this.m_Properties != null) {
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                this.m_PropertyScrollPos = EditorGUILayout.BeginScrollView(this.m_PropertyScrollPos);
                foreach (var property in this.m_Properties) {
                    if (property.isColorProperty) {
                        var properties = property.valuePropertiesJson;
                        int alpha = Util.GetIntProperty(properties, "alpha");
                        int red = Util.GetIntProperty(properties, "red");
                        int green = Util.GetIntProperty(properties, "green");

                        int blue = Util.GetIntProperty(properties, "blue");
                        var color = new Color(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
                        EditorGUILayout.ColorField(property.name, color, GUILayout.ExpandWidth(true));
                    }
                    else {
                        EditorGUILayout.TextField(property.name, property.description);
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) {
                if (splitterRect.yMax > this.m_Window.position.height) {
                    this.m_SplitOffset -= splitterRect.yMax - this.m_Window.position.height;
                    this.m_Window.Repaint();
                }
            }
        }


        public void LoadTree() {
            var node = this.treeType == WidgetTreeType.Widget
                ? this.m_InspectorService.getRootWidgetSummaryTree(this.m_GroupName)
                : this.m_InspectorService.getRootRenderObject(this.m_GroupName);
            this.m_TreeView.node = node;
        }

        public void MarkNeedReload() {
            this.m_TreeView.node = null;
            this.m_NeedSelectionUpdate = true;
        }

        public void Update() {
            if (!this.m_VisibleToUser) {
                return;
            }

            if (this.m_TreeView.node == null) {
                this.LoadTree();
            }

            this.updateSelection();
            this.updateDetailTree();

            if (this.treeType == WidgetTreeType.Render &&
                Timer.timespanSinceStartup - this.m_LastPropertyRefresh > TimeSpan.FromMilliseconds(200)) {
                this.m_LastPropertyRefresh = Timer.timespanSinceStartup;
                this.m_Properties = this.m_SelectedNodeRef == null
                    ? new List<DiagnosticsNode>()
                    : this.m_InspectorService.getProperties(this.m_SelectedNodeRef, this.m_GroupName);
                this.m_Properties = this.m_Properties.Where((p) => p.level != DiagnosticLevel.hidden).ToList();

                this.m_Window.Repaint();
            }
        }


        void OnNodeSelectionChanged(DiagnosticsNode node) {
            this.m_SelectedNodeRef = node == null ? null : node.diagnosticRef;
            this.m_InspectorService.setSelection(node == null ? null : node.valueRef, this.m_GroupName);
            this.m_NeedDetailUpdate = this.m_DetailTreeView != null;
        }

        void handleSelectionChanged() {
            this.m_NeedSelectionUpdate = true;
            this.m_NeedDetailUpdate = true;
        }

        void updateSelection() {
            if (!this.m_NeedSelectionUpdate) {
                return;
            }

            this.m_NeedSelectionUpdate = false;
            var diagnosticsNode = this.m_InspectorService.getSelection(this.m_TreeView.selectedNode, this.treeType,
                true, this.m_GroupName);
            this.m_SelectedNodeRef = diagnosticsNode == null ? null : diagnosticsNode.diagnosticRef;

            if (diagnosticsNode != null) {
                var item = this.m_TreeView.getTreeItemByValueRef(diagnosticsNode.valueRef);
                if (item == null) {
                    this.LoadTree();
                    item = this.m_TreeView.getTreeItemByValueRef(diagnosticsNode.valueRef);
                }

                if (item != null) {
                    this.m_TreeView.SetSelection(new List<int> {item.id}, TreeViewSelectionOptions.RevealAndFrame);
                }
                else {
                    this.m_TreeView.SetSelection(new List<int>());
                }

                this.m_TreeView.Repaint();
            }
        }

        void updateDetailTree() {
            D.assert(!this.m_NeedSelectionUpdate);
            if (!this.m_NeedDetailUpdate) {
                return;
            }

            if (this.m_DetailTreeView == null) {
                return;
            }

            this.m_NeedDetailUpdate = false;
            if (this.m_SelectedNodeRef == null) {
                this.m_DetailTreeView.node = null;
            }
            else {
                this.m_DetailTreeView.node =
                    this.m_InspectorService.getDetailsSubtree(this.m_SelectedNodeRef, this.m_GroupName);
            }

            this.m_DetailTreeView.ExpandAll();
        }

        void splitGUI(Rect splitterRect) {
            var id = GUIUtility.GetControlID("inpectorPannelSplitter".GetHashCode(), FocusType.Passive);
            switch (Event.current.GetTypeForControl(id)) {
                case EventType.MouseDown:
                    if (splitterRect.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = id;
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id) {
                        this.m_SplitOffset += Event.current.delta.y;
                        this.m_Window.Repaint();
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id) {
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
    public class PanelState {
        public WidgetTreeType treeType;
        public float splitOffset;
    }
}
#endif