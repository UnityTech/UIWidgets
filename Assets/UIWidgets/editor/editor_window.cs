using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIWidgets.async;
using UIWidgets.flow;
using UIWidgets.service;
using UIWidgets.rendering;
using UIWidgets.ui;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.editor {  
    
    public class WindowAdapter : Window {
        
        private static List<WindowAdapter> _windowAdapters = new List<WindowAdapter>();
        private bool _alive;
        public static IEnumerable<WindowAdapter> windowAdapters
        {
            get { return _windowAdapters; }
        }

        public WindowAdapter(EditorWindow editorWindow)
        {
            this._alive = true;
            this.editorWindow = editorWindow;
            this.editorWindow.wantsMouseMove = true;
            this.editorWindow.wantsMouseEnterLeaveWindow = true;

            this._devicePixelRatio = EditorGUIUtility.pixelsPerPoint;

            this._lastPosition = editorWindow.position;
            this._physicalSize = new Size(
                this._lastPosition.width * EditorGUIUtility.pixelsPerPoint,
                this._lastPosition.height * EditorGUIUtility.pixelsPerPoint);

            instance = this;
            try {
                this._binding = new WidgetsBinding();
            }
            finally {
                instance = null;
            }
            this._rasterCache = new RasterCache();
            _windowAdapters.Add(this);
        }

        public bool alive
        {
            get { return _alive; }
        }
        
        public void Destory()
        {
            var index = _windowAdapters.FindIndex(w => w == this);
            if (index >= 0)
            {
                _windowAdapters.RemoveAt(index);
            }

            this._alive = false;
        }
        
        public readonly EditorWindow editorWindow;

        public WidgetInspectorService widgetInspectorService
        {
            get { return _binding.widgetInspectorService; }
        }
        

        readonly WidgetsBinding _binding;

        readonly RasterCache _rasterCache;

        Rect _lastPosition;
        readonly DateTime _epoch = new DateTime(Stopwatch.GetTimestamp());
        readonly MicrotaskQueue _microtaskQueue = new MicrotaskQueue();
        readonly TimerProvider _timerProvider = new TimerProvider();
        readonly TextInput _textInput = new TextInput();

        public void OnGUI() {
            instance = this;
            WidgetsBinding.instance = this._binding;

            try {
                this.doOnGUI();
            }
            finally {
                instance = null;
                WidgetsBinding.instance = null;
            }
        }

        public void WithBinding(Action fn)
        {
            instance = this;
            WidgetsBinding.instance = this._binding;
            try
            {
                fn();
            }
            finally {
                instance = null;
                WidgetsBinding.instance = null;
            }
        }
        
        public T WithBindingFunc<T>(Func<T> fn)
        {
            instance = this;
            WidgetsBinding.instance = this._binding;
            try
            {
                return fn();
            }
            finally {
                instance = null;
                WidgetsBinding.instance = null;
            }
        }

        private void doOnGUI() {
            var evt = Event.current;

            if (evt.type == EventType.Repaint) {
                if (this.onBeginFrame != null) {
                    this.onBeginFrame(new DateTime(Stopwatch.GetTimestamp()) - this._epoch);
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
                } else if (evt.type == EventType.MouseMove)
                {
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.hover,
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

            if (_textInput != null) {
                _textInput.OnGUI();
            }
        }

        public void Update() {
            Window.instance = this;
            WidgetsBinding.instance = this._binding;

            try {
                this.doUpdate();
            }
            finally {
                Window.instance = null;
                WidgetsBinding.instance = null;
            }
        }

        private void doUpdate() {
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

            var prerollContext = new PrerollContext {
                rasterCache = this._rasterCache
            };
            layer.preroll(prerollContext, Matrix4x4.identity);

            var paintContext = new PaintContext {
                canvas = new CanvasImpl()
            };
            layer.paint(paintContext);

            this._rasterCache.sweepAfterFrame();
        }

        public override void scheduleMicrotask(Action callback) {
            this._microtaskQueue.scheduleMicrotask(callback);
        }

        public override void flushMicrotasks() {
            this._microtaskQueue.flushMicrotasks();
        }

        public override Timer run(TimeSpan duration, Action callback, bool periodic = false)
        {
            return periodic
                ? this._timerProvider.periodic(duration, callback)
                : this._timerProvider.run(duration, callback);
        }

        public void attachRootRenderBox(RenderBox root) {
            Window.instance = this;
            WidgetsBinding.instance = this._binding;

            try {
                this._binding.renderView.child = root;
            }
            finally {
                Window.instance = null;
                WidgetsBinding.instance = null;
            }
        }

        public void attachRootWidget(Widget root) {
            Window.instance = this;
            WidgetsBinding.instance = this._binding;

            try {
                this._binding.attachRootWidget(root);
            }
            finally {
                Window.instance = null;
                WidgetsBinding.instance = null;
            }
        }

        public override TextInput textInput {
            get { return _textInput; }
        }
    }
}