using System;
using System.Collections.Generic;
using System.Reflection;
using UIWidgets.foundation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIWidgets.debugger
{
    public class InspectorTreeView : TreeView
    {
        private static int m_NextId = 0;
        private DiagnosticsNode m_Node;

        private readonly Dictionary<InspectorInstanceRef, InspectorTreeItem> m_ValueToNode =
            new Dictionary<InspectorInstanceRef, InspectorTreeItem>();

        public delegate void NodeSelectionChanged(DiagnosticsNode node);

        public NodeSelectionChanged onNodeSelectionChanged;

        public InspectorTreeView(TreeViewState state) : base(state)
        {
            // todo better way to enable horizontal scroll in treeview
            setFieldValueRelection(this, new List<string> {"m_GUI", "m_UseHorizontalScroll"}, true);
            useScrollView = true;
            showBorder = true;
        }

        public DiagnosticsNode node
        {
            set
            {
                m_Node = value;
                Reload();
            }

            get { return m_Node; }
        }

        public DiagnosticsNode selectedNode
        {
            get
            {
                var selection = GetSelection();
                if (selection.Count <= 0)
                {
                    return null;
                }

                var item = FindItem(selection[0], rootItem) as InspectorTreeItem;
                return item == null ? null : item.node;
            }
        }

        public InspectorTreeItem getTreeItemByValueRef(InspectorInstanceRef instanceRef, TreeViewItem from = null)
        {
            InspectorTreeItem item;
            m_ValueToNode.TryGetValue(instanceRef, out item);
            return item;
        }

        protected override void RowGUI(TreeView.RowGUIArgs args)
        {
            var item = args.item as InspectorTreeItem;
            if (item == null || item.node == null || !item.node.isProperty)
            {
                base.RowGUI(args);
                return;
            }

            var node = item.node;
            var rect = args.rowRect;
            rect.xMin += GetContentIndent(item);


            var xoffset = rect.xMin;
            if (node.showName && !string.IsNullOrEmpty(node.name))
            {
                xoffset = labelGUI(xoffset, rect, string.Format("{0}{1} ", node.name, node.separator));
            }

            var properties = node.valuePropertiesJson;
            var iconSize = rect.height;
            if (node.isColorProperty)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    int alpha = Util.GetIntProperty(properties, "alpha");
                    int red = Util.GetIntProperty(properties, "red");
                    int green = Util.GetIntProperty(properties, "green");

                    int blue = Util.GetIntProperty(properties, "blue");
                    var color = new UnityEngine.Color(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
                    Util.DrawColorIcon(new Rect(xoffset, rect.yMin, rect.height, rect.height), color);
                }
                xoffset += iconSize;
            }
            labelGUI(xoffset, rect, node.description);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            DiagnosticsNode node = null;
            if (selectedIds.Count > 0)
            {
                var id = selectedIds[0];
                var item = FindItem(id, rootItem) as InspectorTreeItem;
                if (item != null)
                {
                    node = item.node;
                }
            }

            if (onNodeSelectionChanged != null)
            {
                onNodeSelectionChanged(node);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            m_NextId = 0;
            m_ValueToNode.Clear();
            var root = new TreeViewItem(m_NextId++, -1);
            root.children = new List<TreeViewItem>();
            if (m_Node != null)
            {
                root.AddChild(build(m_Node, false));
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        private float labelGUI(float offset, Rect rowRect, string text)
        {
            float minWidth, maxWidth;
            GUI.skin.label.CalcMinMaxWidth(new GUIContent(text), out minWidth, out maxWidth);
            rowRect.xMin = offset;
            rowRect.width = maxWidth;
            GUI.Label(rowRect, text);
            return rowRect.xMax;
        }

        private InspectorTreeItem build(DiagnosticsNode node, bool inProperty)
        {
            D.assert(node != null);
            var item = new InspectorTreeItem(node, m_NextId++);
            inProperty = inProperty || node.isProperty;
            if (!inProperty && node.valueRef != null)
            {
                m_ValueToNode[node.valueRef] = item;
            }

            foreach (var propertyNode in node.inlineProperties)
            {
                item.AddChild(build(propertyNode, inProperty));
            }

            var children = node.children;
            foreach (var childNode in children)
            {
                item.AddChild(build(childNode, inProperty));
            }

            return item;
        }

        private void setFieldValueRelection(object obj, List<string> fields, object value)
        {
            for (var i = 0; i < fields.Count; ++i)
            {
                if (obj == null)
                {
                    return;
                }

                FieldInfo fieldInfo = null;
                for (var type = obj.GetType(); type != null && fieldInfo == null; type = type.BaseType)
                {
                    fieldInfo = type.GetField(fields[i],
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }

                if (fieldInfo == null)
                {
                    return;
                }

                if (i + 1 < fields.Count)
                {
                    obj = fieldInfo.GetValue(obj);
                }
                else
                {
                    fieldInfo.SetValue(obj, value);
                }
            }
        }
    }

    public class InspectorTreeItem : TreeViewItem
    {
        public readonly DiagnosticsNode node;

        public InspectorTreeItem(DiagnosticsNode node, int id) : base(id)
        {
            this.node = node;
        }


        public override string displayName
        {
            get { return node.name + node.description; }
        }
    }

    public static class Util
    {
        private const float colorIconMargin = 1.0f;

        public static int GetIntProperty(Dictionary<string, object> properties, string name)
        {
            object val;
            properties.TryGetValue(name, out val);
            if (val == null)
            {
                return -1;
            }

            return Convert.ToInt32(val);
        }

        public static void DrawColorIcon(Rect rect, Color color)
        {
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