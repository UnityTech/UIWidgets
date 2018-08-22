using System;
using UIWidgets.flow;
using UIWidgets.rendering;
using UIWidgets.ui;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.editor {
    public class WindowAdapter : Window {
        public WindowAdapter(EditorWindow editorWindow) {
            this.editorWindow = editorWindow;
            this._devicePixelRatio = EditorGUIUtility.pixelsPerPoint;
            this._lastPosition = editorWindow.position;
            this._physicalSize = new Size(this._lastPosition.width, this._lastPosition.height);
        }

        public EditorWindow editorWindow;
        public Rect _lastPosition;
        public readonly DateTime _epoch = DateTime.Now;

        public void OnGUI() {
            if (this.onBeginFrame != null) {
                this.onBeginFrame(DateTime.Now - this._epoch);
            }

            if (this.onDrawFrame != null) {
                this.onDrawFrame();
            }
        }

        public void Update() {
            bool dirty = false;
            if (this._devicePixelRatio != EditorGUIUtility.pixelsPerPoint) {
                this._devicePixelRatio = EditorGUIUtility.pixelsPerPoint;
                dirty = true;
            }

            if (this._lastPosition != this.editorWindow.position) {
                this._lastPosition = this.editorWindow.position;
                this._physicalSize = new Size(this._lastPosition.width, this._lastPosition.height);
                dirty = true;
            }

            if (dirty) {
                if (this._onMetricsChanged != null) {
                    this._onMetricsChanged();
                }
            }
        }

        public override void scheduleFrame() {
            if (this.editorWindow != null) {
                this.editorWindow.Repaint();
            }
        }

        public override void render(Scene scene) {
            var layer = scene.takeLayer();

            var prerollContext = new PrerollContext();
            layer.preroll(prerollContext, Matrix4x4.identity);

            var paintContext = new PaintContext();
            layer.paint(paintContext);
        }
    }

    public class EditorWindow : UnityEditor.EditorWindow {
        protected virtual void OnGUI() {
            if (this.windowAdapter != null) {
                this.windowAdapter.OnGUI();
            }
        }

        protected virtual void Update() {
            if (this.windowAdapter != null) {
                this.windowAdapter.Update();
            }
        }

        protected virtual void Awake() {
            this.windowAdapter = new WindowAdapter(this);
        }

        protected void OnDestroy() {
            this.windowAdapter = null;
        }

        public WindowAdapter windowAdapter;

        public void runWidget(Widget root) {
        }

        public void runRenderBox(RenderBox root) {
            new RendererBindingImpl(this.windowAdapter, root);
        }
    }
}