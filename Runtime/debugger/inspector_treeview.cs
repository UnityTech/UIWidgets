#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.UIWidgets.foundation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class InspectorTreeView : TreeView {
        static int m_NextId = 0;
        DiagnosticsNode m_Node;

        readonly Dictionary<InspectorInstanceRef, InspectorTreeItem> m_ValueToNode =
            new Dictionary<InspectorInstanceRef, InspectorTreeItem>();

        public delegate void NodeSelectionChanged(DiagnosticsNode node);

        public NodeSelectionChanged onNodeSelectionChanged;

        public InspectorTreeView(TreeViewState state) : base(state) {
            // todo better way to enable horizontal scroll in treeview
            this.setFieldValueRelection(this, new List<string> {"m_GUI", "m_UseHorizontalScroll"}, true);
            this.useScrollView = true;
            this.showBorder = true;
        }

        public DiagnosticsNode node {
            set {
                this.m_Node = value;
                this.Reload();
            }

            get { return this.m_Node; }
        }

        public DiagnosticsNode selectedNode {
            get {
                var selection = this.GetSelection();
                if (selection.Count <= 0) {
                    return null;
                }

                var item = this.FindItem(selection[0], this.rootItem) as InspectorTreeItem;
                return item == null ? null : item.node;
            }
        }

        public InspectorTreeItem getTreeItemByValueRef(InspectorInstanceRef instanceRef, TreeViewItem from = null) {
            InspectorTreeItem item;
            this.m_ValueToNode.TryGetValue(instanceRef, out item);
            return item;
        }

        protected override void RowGUI(RowGUIArgs args) {
            var item = args.item as InspectorTreeItem;
            if (item == null || item.node == null || !item.node.isProperty) {
                base.RowGUI(args);
                return;
            }

            var node = item.node;
            var rect = args.rowRect;
            rect.xMin += this.GetContentIndent(item);


            var xoffset = rect.xMin;
            if (node.showName && !string.IsNullOrEmpty(node.name)) {
                xoffset = this.labelGUI(xoffset, rect, $"{node.name}{node.separator} ");
            }

            var properties = node.valuePropertiesJson;
            var iconSize = rect.height;
            if (node.isColorProperty) {
                if (Event.current.type == EventType.Repaint) {
                    int alpha = Util.GetIntProperty(properties, "alpha");
                    int red = Util.GetIntProperty(properties, "red");
                    int green = Util.GetIntProperty(properties, "green");

                    int blue = Util.GetIntProperty(properties, "blue");
                    var color = new Color(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
                    Util.DrawColorIcon(new Rect(xoffset, rect.yMin, rect.height, rect.height), color);
                }

                xoffset += iconSize;
            }

            this.labelGUI(xoffset, rect, node.description.Replace("\n", " "));
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            DiagnosticsNode node = null;
            if (selectedIds.Count > 0) {
                var id = selectedIds[0];
                var item = this.FindItem(id, this.rootItem) as InspectorTreeItem;
                if (item != null) {
                    node = item.node;
                }
            }

            if (this.onNodeSelectionChanged != null) {
                this.onNodeSelectionChanged(node);
            }
        }

        protected override TreeViewItem BuildRoot() {
            m_NextId = 0;
            this.m_ValueToNode.Clear();
            var root = new TreeViewItem(m_NextId++, -1);
            root.children = new List<TreeViewItem>();
            if (this.m_Node != null) {
                root.AddChild(this.build(this.m_Node, false));
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        float labelGUI(float offset, Rect rowRect, string text) {
            float minWidth, maxWidth;
            GUI.skin.label.CalcMinMaxWidth(new GUIContent(text), out minWidth, out maxWidth);
            rowRect.xMin = offset;
            rowRect.width = maxWidth;
            GUI.Label(rowRect, text);
            return rowRect.xMax;
        }

        InspectorTreeItem build(DiagnosticsNode node, bool inProperty) {
            D.assert(node != null);
            var item = new InspectorTreeItem(node, m_NextId++);
            inProperty = inProperty || node.isProperty;
            if (!inProperty && node.valueRef != null) {
                this.m_ValueToNode[node.valueRef] = item;
            }

            foreach (var propertyNode in node.inlineProperties) {
                item.AddChild(this.build(propertyNode, inProperty));
            }

            var children = node.children;
            foreach (var childNode in children) {
                item.AddChild(this.build(childNode, inProperty));
            }

            return item;
        }

        void setFieldValueRelection(object obj, List<string> fields, object value) {
            for (var i = 0; i < fields.Count; ++i) {
                if (obj == null) {
                    return;
                }

                FieldInfo fieldInfo = null;
                for (var type = obj.GetType(); type != null && fieldInfo == null; type = type.BaseType) {
                    fieldInfo = type.GetField(fields[i],
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }

                if (fieldInfo == null) {
                    return;
                }

                if (i + 1 < fields.Count) {
                    obj = fieldInfo.GetValue(obj);
                }
                else {
                    fieldInfo.SetValue(obj, value);
                }
            }
        }
    }

    public class InspectorTreeItem : TreeViewItem {
        public readonly DiagnosticsNode node;

        public InspectorTreeItem(DiagnosticsNode node, int id) : base(id) {
            this.node = node;
        }


        public override string displayName {
            get {
                if (this.node.showName && !string.IsNullOrEmpty(this.node.name)) {
                    return $"{this.node.name}{this.node.separator} {this.node.description}";
                }

                return this.node.description;
            }
        }
    }

    public static class Util {
        const float colorIconMargin = 1.0f;

        public static int GetIntProperty(Dictionary<string, object> properties, string name) {
            object val = null;

            if (properties != null) {
                properties.TryGetValue(name, out val);
            }

            if (val == null) {
                return -1;
            }

            return Convert.ToInt32(val);
        }

        public static void DrawColorIcon(Rect rect, Color color) {
            var innerRect = new Rect(rect.x + colorIconMargin, rect.y + colorIconMargin,
                rect.width - 2 * colorIconMargin,
                rect.height - 2 * colorIconMargin);

            GUI.DrawTexture(innerRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0,
                Color.white, 0, 0);

            GUI.DrawTexture(new Rect(innerRect.x, innerRect.y, innerRect.width / 2,
                    innerRect.height / 2), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0,
                Color.gray, 0, 0);
            GUI.DrawTexture(new Rect(innerRect.x + innerRect.width / 2, innerRect.y + innerRect.height / 2,
                    innerRect.width / 2,
                    innerRect.height / 2), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0,
                Color.gray, 0, 0);

            GUI.DrawTexture(innerRect,
                EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0,
                color, 0, 0);
        }
    }
}
#endif