using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.debugger {
    public delegate void SelectionChanged();

    public class InspectorService {
        public readonly WindowAdapter window;

        public SelectionChanged selectionChanged;

        readonly WidgetInspectorService _widgetInspectorService;

        public InspectorService(WindowAdapter window) {
            this.window = window;
            this._widgetInspectorService = window.widgetInspectorService;
            this._widgetInspectorService.developerInspect += this.developerInspect; // todo dispose
        }

        public bool active {
            get { return this.window.alive; }
        }

        public bool debugEnabled {
            get { return D.debugEnabled; }
        }

        public void close() {
            this.setShowInspect(false);
            this._widgetInspectorService.developerInspect -= this.developerInspect;
        }

        public DiagnosticsNode getRootWidgetSummaryTree(string groupName) {
            return this.toNode(this.window.withBindingFunc(() =>
                this._widgetInspectorService.getRootWidgetSummaryTree(groupName)));
        }

        public DiagnosticsNode getRootWidget(string groupName) {
            return this.toNode(this.window.withBindingFunc(() =>
                this._widgetInspectorService.getRootWidget(groupName)));
        }

        public DiagnosticsNode getRootRenderObject(string groupName) {
            return this.toNode(this.window.withBindingFunc(() =>
                this._widgetInspectorService.getRootRenderObject(groupName)));
        }

        public DiagnosticsNode getDetailsSubtree(InspectorInstanceRef instanceRef, string groupName) {
            return this.toNode(this.window.withBindingFunc(() =>
                this._widgetInspectorService.getDetailsSubtree(instanceRef.id, groupName)));
        }

        public DiagnosticsNode getSelection(DiagnosticsNode previousSelection, WidgetTreeType treeType, bool localOnly,
            string groupName) {
            InspectorInstanceRef previousSelectionRef =
                previousSelection == null ? null : previousSelection.diagnosticRef;
            string previousSelectionId = previousSelectionRef == null ? null : previousSelectionRef.id;
            DiagnosticsNode result = null;

            this.window.withBinding(() => {
                switch (treeType) {
                    case WidgetTreeType.Widget:
                        result = localOnly
                            ? this.toNode(
                                this._widgetInspectorService.getSelectedSummaryWidget(previousSelectionId, groupName))
                            : this.toNode(
                                this._widgetInspectorService.getSelectedWidget(previousSelectionId, groupName));
                        break;
                    case WidgetTreeType.Render:
                        result = this.toNode(
                            this._widgetInspectorService.getSelectedRenderObject(previousSelectionId, groupName));
                        break;
                }
            });


            if (result != null && result.diagnosticRef == previousSelectionRef) {
                return previousSelection;
            }
            else {
                return result;
            }
        }

        public bool setSelection(InspectorInstanceRef inspectorInstanceRef, string groupName) {
            return this.window.withBindingFunc(() =>
                this._widgetInspectorService.setSelectionById(inspectorInstanceRef.id, groupName));
        }

        public void setShowInspect(bool show) {
            this.window.withBinding(() => { this._widgetInspectorService.debugShowInspector = show; });
        }

        public bool getShowInspect() {
            return this.window.withBindingFunc(() => this._widgetInspectorService.debugShowInspector);
        }

        public List<DiagnosticsNode> getProperties(InspectorInstanceRef inspectorInstanceRef, string groupName) {
            var list = this.window.withBindingFunc(() =>
                this._widgetInspectorService.getProperties(inspectorInstanceRef.id, groupName));
            return list.Select(json => this.toNode(json, isProperty: true)).ToList();
        }

        public void disposeGroup(string groupName) {
            this.window.withBinding(() => this._widgetInspectorService.disposeGroup(groupName));
        }

        DiagnosticsNode toNode(Dictionary<string, object> json, bool isProperty = false) {
            if (json == null) {
                return null;
            }

            return new DiagnosticsNode(json, isProperty);
        }


        void developerInspect() {
            if (this.selectionChanged != null) {
                this.selectionChanged();
            }
        }
    }

    public enum WidgetTreeType {
        Widget,
        Render
    }
}