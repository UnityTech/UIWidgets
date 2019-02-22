using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.editor {
#if UNITY_EDITOR
    public abstract class UIWidgetsEditorWindow : EditorWindow {
        WindowAdapter _windowAdapter;

        public UIWidgetsEditorWindow() {
            this.wantsMouseMove = true;
            this.wantsMouseEnterLeaveWindow = true;
        }

        void OnEnable() {
            if (this._windowAdapter == null) {
                this._windowAdapter = new EditorWindowAdapter(this);
            }

            this._windowAdapter.OnEnable();

            var rootRenderBox = this.rootRenderBox();
            if (rootRenderBox != null) {
                this._windowAdapter.attachRootRenderBox(rootRenderBox);
                return;
            }

            this._windowAdapter.attachRootWidget(this.rootWidget());
        }

        void OnDisable() {
            this._windowAdapter.OnDisable();
        }

        void OnGUI() {
            this._windowAdapter.OnGUI(Event.current);
        }

        void Update() {
            this._windowAdapter.Update();
        }

        protected virtual RenderBox rootRenderBox() {
            return null;
        }

        protected abstract Widget rootWidget();
    }

    public class EditorWindowAdapter : WindowAdapter {
        public readonly EditorWindow editorWindow;

        public EditorWindowAdapter(EditorWindow editorWindow) {
            this.editorWindow = editorWindow;
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            this.editorWindow.Repaint();
        }

        public override GUIContent titleContent {
            get { return this.editorWindow.titleContent; }
        }

        protected override double queryDevicePixelRatio() {
            return EditorGUIUtility.pixelsPerPoint;
        }

        protected override Vector2 queryWindowSize() {
            return this.editorWindow.position.size;
        }
    }

#endif

    public abstract class WindowAdapter : Window {
        static readonly List<WindowAdapter> _windowAdapters = new List<WindowAdapter>();

        public static IEnumerable<WindowAdapter> windowAdapters {
            get { return _windowAdapters; }
        }

        public WidgetInspectorService widgetInspectorService {
            get {
                D.assert(this._binding != null);
                return this._binding.widgetInspectorService;
            }
        }

        WidgetsBinding _binding;
        float _lastWindowWidth;
        float _lastWindowHeight;

        readonly TimeSpan _epoch = new TimeSpan(Stopwatch.GetTimestamp());
        readonly MicrotaskQueue _microtaskQueue = new MicrotaskQueue();
        readonly TimerProvider _timerProvider = new TimerProvider();
        readonly TextInput _textInput = new TextInput();
        readonly Rasterizer _rasterizer = new Rasterizer();
        readonly ScrollInput _scrollInput = new ScrollInput();


        bool _regenerateLayerTree;
        Surface _surface;

        bool _alive;

        public bool alive {
            get { return this._alive; }
        }

        protected virtual void updateSafeArea() {
        }

        public void OnEnable() {
            this._devicePixelRatio = this.queryDevicePixelRatio();
            var size = this.queryWindowSize();
            this._lastWindowWidth = size.x;
            this._lastWindowHeight = size.y;
            this._physicalSize = new Size(
                this._lastWindowWidth * this._devicePixelRatio,
                this._lastWindowHeight * this._devicePixelRatio);

            this.updateSafeArea();
            D.assert(this._surface == null);
            this._surface = this.createSurface();

            this._rasterizer.setup(this._surface);
            _windowAdapters.Add(this);
            this._alive = true;
        }

        public void OnDisable() {
            _windowAdapters.Remove(this);
            this._alive = false;

            this._rasterizer.teardown();

            D.assert(this._surface != null);
            this._surface.Dispose();
            this._surface = null;
        }

        public override IDisposable getScope() {
            instance = this;
            if (this._binding == null) {
                this._binding = new WidgetsBinding();
            }

            WidgetsBinding.instance = this._binding;

            return new WindowDisposable(this);
        }

        class WindowDisposable : IDisposable {
            readonly WindowAdapter _window;

            public WindowDisposable(WindowAdapter window) {
                this._window = window;
            }

            public void Dispose() {
                D.assert(instance == this._window);
                instance = null;

                D.assert(WidgetsBinding.instance == this._window._binding);
                WidgetsBinding.instance = null;
            }
        }

        public void postPointerEvents(List<PointerData> data) {
            this.withBinding(() => { this.onPointerEvent(new PointerDataPacket(data)); });
        }

        public void postPointerEvent(PointerData data) {
            this.postPointerEvents(new List<PointerData>() {data});
        }

        public void withBinding(Action fn) {
            using (this.getScope()) {
                fn();
            }
        }

        public T withBindingFunc<T>(Func<T> fn) {
            using (this.getScope()) {
                return fn();
            }
        }

        protected bool displayMetricsChanged() {
            if (this._devicePixelRatio != this.queryDevicePixelRatio()) {
                return true;
            }
            var size = this.queryWindowSize();
            if (this._lastWindowWidth != size.x
                || this._lastWindowHeight != size.y) {
                return true;
            }

            return false;
        }

        public virtual void OnGUI(Event evt = null) {
            evt = evt ?? Event.current;
            using (this.getScope()) {
                if (this.displayMetricsChanged()) {
                    this._devicePixelRatio = this.queryDevicePixelRatio();
                    var size = this.queryWindowSize();
                    this._lastWindowWidth = size.x;
                    this._lastWindowHeight = size.y;
                    this._physicalSize = new Size(
                        this._lastWindowWidth * this._devicePixelRatio,
                        this._lastWindowHeight * this._devicePixelRatio);

                    this.updateSafeArea();
                    if (this.onMetricsChanged != null) {
                        this.onMetricsChanged();
                    }
                }

                this._doOnGUI(evt);
            }
        }

        public virtual GUIContent titleContent {
            get { return null; }
        }

        protected abstract double queryDevicePixelRatio();
        protected abstract Vector2 queryWindowSize();

        protected virtual Surface createSurface() {
            return new EditorWindowSurface();
        }


        void _beginFrame() {
            if (this.onBeginFrame != null) {
                this.onBeginFrame(new TimeSpan(Stopwatch.GetTimestamp()) - this._epoch);
            }

            this.flushMicrotasks();

            if (this.onDrawFrame != null) {
                this.onDrawFrame();
            }
        }

        void _doOnGUI(Event evt) {
            if (evt.type == EventType.Repaint) {
                if (this._regenerateLayerTree) {
                    this._regenerateLayerTree = false;
                    this._beginFrame();
                }
                else {
                    this._rasterizer.drawLastLayerTree();
                }

                return;
            }

            if (this.onPointerEvent != null) {
                PointerData pointerData = null;

                if (evt.type == EventType.MouseDown) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.down,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.up,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseDrag) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.move,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseMove) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.hover,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.ScrollWheel) {
                    this._scrollInput.onScroll((float) (-evt.delta.x * this._devicePixelRatio),
                        (float) (-evt.delta.y * this._devicePixelRatio),
                        (float) (evt.mousePosition.x * this._devicePixelRatio),
                        (float) (evt.mousePosition.y * this._devicePixelRatio),
                        evt.button
                    );
                }

                if (pointerData != null) {
                    this.onPointerEvent(new PointerDataPacket(new List<PointerData> {
                        pointerData
                    }));
                }
            }

            if (this._textInput != null) {
                this._textInput.keyboardManager.OnGUI();
            }
        }

        void _updateScrollInput() {
            var deltaScroll = this._scrollInput.getScrollDelta();

            if (deltaScroll == Vector2.zero) {
                return;
            }

            PointerData pointerData = new ScrollData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.scroll,
                kind: PointerDeviceKind.mouse,
                device: this._scrollInput.getDeviceId(),
                physicalX: this._scrollInput.getPointerPosX(),
                physicalY: this._scrollInput.getPointerPosY(),
                scrollX: deltaScroll.x,
                scrollY: deltaScroll.y
            );

            this.onPointerEvent(new PointerDataPacket(new List<PointerData> {
                pointerData
            }));
        }

        public void Update() {
            Timer.update();

            using (this.getScope()) {
                this._updateScrollInput();
                if (this._textInput != null) {
                    this._textInput.keyboardManager.Update();
                }
            
                this._timerProvider.update(this.flushMicrotasks);
                this.flushMicrotasks();
            }
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            if (regenerateLayerTree) {
                this._regenerateLayerTree = true;
            }
        }

        public override void render(Scene scene) {
            var layerTree = scene.takeLayerTree();
            if (layerTree == null) {
                return;
            }

            if (this._physicalSize.isEmpty) {
                return;
            }

            layerTree.frameSize = this._physicalSize;
            layerTree.devicePixelRatio = this._devicePixelRatio;
            this._rasterizer.draw(layerTree);
        }

        public override void scheduleMicrotask(Action callback) {
            this._microtaskQueue.scheduleMicrotask(callback);
        }

        public override void flushMicrotasks() {
            this._microtaskQueue.flushMicrotasks();
        }

        public override Timer run(TimeSpan duration, Action callback, bool periodic = false) {
            return periodic
                ? this._timerProvider.periodic(duration, callback)
                : this._timerProvider.run(duration, callback);
        }

        public override Timer runInMain(Action callback) {
            return this._timerProvider.runInMain(callback);
        }

        public void attachRootRenderBox(RenderBox root) {
            using (this.getScope()) {
                this._binding.renderView.child = root;
            }
        }

        public void attachRootWidget(Widget root) {
            using (this.getScope()) {
                this._binding.attachRootWidget(root);
            }
        }

        public override TextInput textInput {
            get { return this._textInput; }
        }
    }
}