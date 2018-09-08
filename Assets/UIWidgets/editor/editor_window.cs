using System;
using System.Collections.Generic;
using UIWidgets.async;
using UIWidgets.flow;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.editor {
    public class WindowAdapter : Window {
        public WindowAdapter(EditorWindow editorWindow) {
            this.editorWindow = editorWindow;
            this.editorWindow.wantsMouseMove = false;
            this.editorWindow.wantsMouseEnterLeaveWindow = false;

            this._devicePixelRatio = EditorGUIUtility.pixelsPerPoint;

            this._lastPosition = editorWindow.position;
            this._physicalSize = new Size(
                this._lastPosition.width * EditorGUIUtility.pixelsPerPoint, 
                this._lastPosition.height * EditorGUIUtility.pixelsPerPoint);
        }

        public readonly EditorWindow editorWindow;
        
        Rect _lastPosition;
        readonly DateTime _epoch = DateTime.Now;
        readonly MicrotaskQueue _microtaskQueue = new MicrotaskQueue();
        readonly TimerProvider _timerProvider = new TimerProvider();

        public void OnGUI() {
            var evt = Event.current;

            if (evt.type == EventType.Repaint) {
                if (this.onBeginFrame != null) {
                    this.onBeginFrame(DateTime.Now - this._epoch);
                }

                this.flushMicrotasks();

                if (this.onDrawFrame != null) {
                    this.onDrawFrame();
                }

                return;
            }

            if (this.onPointerEvent != null) {
                PointerData pointerData = null;

                if (evt.type == EventType.MouseDown) {
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.down,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                } else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.up,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                } else if (evt.type == EventType.MouseDrag) {
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.move,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }

                if (pointerData != null) {
                    this.onPointerEvent(new PointerDataPacket(new List<PointerData> {
                        pointerData
                    }));
                }
            }
        }

        public void Update() {
            this.flushMicrotasks();

            this._timerProvider.update();

            bool dirty = false;
            if (this._devicePixelRatio != EditorGUIUtility.pixelsPerPoint) {
                dirty = true;
            }

            if (this._lastPosition != this.editorWindow.position) {
                dirty = true;
            }

            if (dirty) {
                this._devicePixelRatio = EditorGUIUtility.pixelsPerPoint;
                this._lastPosition = this.editorWindow.position;
                this._physicalSize = new Size(
                    this._lastPosition.width * EditorGUIUtility.pixelsPerPoint,
                    this._lastPosition.height * EditorGUIUtility.pixelsPerPoint);

                if (this.onMetricsChanged != null) {
                    this.onMetricsChanged();
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

            var paintContext = new PaintContext {canvas = new CanvasImpl()};
            layer.paint(paintContext);
        }

        public override void scheduleMicrotask(Action callback) {
            this._microtaskQueue.scheduleMicrotask(callback);
        }

        public override void flushMicrotasks() {
            this._microtaskQueue.flushMicrotasks();
        }

        public override Timer run(TimeSpan duration, Action callback) {
            return this._timerProvider.run(duration, callback);
        }
    }
}