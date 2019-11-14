using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.editor {
#if UNITY_EDITOR
    public abstract class UIWidgetsEditorWindow : EditorWindow, WindowHost {
        WindowAdapter _windowAdapter;
        
        static readonly List<UIWidgetsEditorWindow> _activeEditorWindows = new List<UIWidgetsEditorWindow>();

        [InitializeOnLoadMethod]
        static void _OnBaseEditorWindowLoaded()
        {
            EditorApplication.quitting += () =>
            {
                foreach (var editorWindow in _activeEditorWindows) {
                    editorWindow.OnDisable();
                }
                
                _activeEditorWindows.Clear();
            };
        }
        
        public UIWidgetsEditorWindow() {
            this.wantsMouseMove = true;
            this.wantsMouseEnterLeaveWindow = true;
            
            _activeEditorWindows.Add(this);
        }
        
        void OnDestroy() {
            if (_activeEditorWindows.Contains(this)) {
                _activeEditorWindows.Remove(this);
            }
        }

        protected virtual void OnEnable() {
            if (this._windowAdapter == null) {
                this._windowAdapter = new EditorWindowAdapter(this);
            }

            this._windowAdapter.OnEnable();

            RenderBox rootRenderBox;
            using (this._windowAdapter.getScope()) {
                rootRenderBox = this.createRenderBox();
            }

            if (rootRenderBox != null) {
                this._windowAdapter.attachRootRenderBox(rootRenderBox);
                return;
            }

            Widget rootWidget;
            using (this._windowAdapter.getScope()) {
                rootWidget = this.createWidget();
            }

            this._windowAdapter.attachRootWidget(rootWidget);
        }

        protected virtual void OnDisable() {
            this._windowAdapter.OnDisable();
        }

        protected virtual void OnGUI() {
            this._windowAdapter.OnGUI(Event.current);
        }

        protected virtual void Update() {
            this._windowAdapter.Update();
        }

        protected virtual RenderBox createRenderBox() {
            return null;
        }

        protected abstract Widget createWidget();

        public Window window {
            get { return this._windowAdapter; }
        }
    }

    public class EditorWindowAdapter : WindowAdapter {
        public readonly EditorWindow editorWindow;

        public EditorWindowAdapter(EditorWindow editorWindow) : base(true) {
            this.editorWindow = editorWindow;
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            this.editorWindow.Repaint();
        }

        protected override bool hasFocus() {
            return EditorWindow.focusedWindow == this.editorWindow;
        }

        public override GUIContent titleContent {
            get { return this.editorWindow.titleContent; }
        }

        protected override float queryDevicePixelRatio() {
            return EditorGUIUtility.pixelsPerPoint;
        }
        
        protected override int queryAntiAliasing() {
            return defaultAntiAliasing;
        }

        protected override Vector2 queryWindowSize() {
            return this.editorWindow.position.size;
        }

        protected override TimeSpan getTime() {
            return TimeSpan.FromSeconds(EditorApplication.timeSinceStartup);
        }

        float? _lastUpdateTime;

        protected override void updateDeltaTime() {
            if (this._lastUpdateTime == null) {
                this._lastUpdateTime = (float) EditorApplication.timeSinceStartup;
            }

            this.deltaTime = (float) EditorApplication.timeSinceStartup - this._lastUpdateTime.Value;
            this.unscaledDeltaTime = this.deltaTime;
            this._lastUpdateTime = (float) EditorApplication.timeSinceStartup;
        }
    }

#endif

    public interface WindowHost {
        Window window { get; }
    }

    public abstract class WindowAdapter : Window {
        static readonly List<WindowAdapter> _windowAdapters = new List<WindowAdapter>();

        public WindowAdapter(bool inEditorWindow = false) {
            this.inEditorWindow = inEditorWindow;
        }

        public static List<WindowAdapter> windowAdapters {
            get { return _windowAdapters; }
        }

        public WidgetInspectorService widgetInspectorService {
            get {
                D.assert(this._binding != null);
                return this._binding.widgetInspectorService;
            }
        }

        internal WidgetsBinding _binding;
        float _lastWindowWidth;
        float _lastWindowHeight;

        bool _viewMetricsChanged;

        readonly MicrotaskQueue _microtaskQueue = new MicrotaskQueue();
        readonly TimerProvider _timerProvider = new TimerProvider();
        readonly Rasterizer _rasterizer = new Rasterizer();
        readonly ScrollInput _scrollInput = new ScrollInput();

        bool _regenerateLayerTree;
        Surface _surface;

        bool _alive;

        public bool alive {
            get { return this._alive; }
        }

        protected virtual TimeSpan getTime() {
            return TimeSpan.FromSeconds(Time.time);
        }

        protected float deltaTime;
        protected float unscaledDeltaTime;

        void updatePhysicalSize() {
            var size = this.queryWindowSize();
            this._physicalSize = new Size(
                size.x * this._devicePixelRatio,
                size.y * this._devicePixelRatio);
        }


        protected virtual void updateDeltaTime() {
            this.deltaTime = Time.unscaledDeltaTime;
            this.unscaledDeltaTime = Time.deltaTime;
        }

        protected virtual void updateSafeArea() {
        }

        public void onViewMetricsChanged() {
            this._viewMetricsChanged = true;
        }

        protected abstract bool hasFocus();

        public void OnEnable() {
            this._devicePixelRatio = this.queryDevicePixelRatio();
            this._antiAliasing = this.queryAntiAliasing();
            this.updatePhysicalSize();
            this.updateSafeArea();
            D.assert(this._surface == null);
            this._surface = this.createSurface();

            this._rasterizer.setup(this._surface);
            _windowAdapters.Add(this);
            this._alive = true;
        }

        public void OnDisable() {
            using (this.getScope()) {
                this._binding.detachRootWidget();
            }

            _windowAdapters.Remove(this);
            this._alive = false;

            this._rasterizer.teardown();

            D.assert(this._surface != null);
            this._surface.Dispose();
            this._surface = null;
        }

        readonly protected bool inEditorWindow;

        public override IDisposable getScope() {
            WindowAdapter oldInstance = (WindowAdapter) _instance;
            _instance = this;

            if (this._binding == null) {
                this._binding = new WidgetsBinding(this.inEditorWindow);
            }

            SchedulerBinding._instance = this._binding;

            return new WindowDisposable(this, oldInstance);
        }

        class WindowDisposable : IDisposable {
            readonly WindowAdapter _window;
            readonly WindowAdapter _oldWindow;

            public WindowDisposable(WindowAdapter window, WindowAdapter oldWindow) {
                this._window = window;
                this._oldWindow = oldWindow;
            }

            public void Dispose() {
                D.assert(_instance == this._window);
                _instance = this._oldWindow;

                D.assert(SchedulerBinding._instance == this._window._binding);
                SchedulerBinding._instance = this._oldWindow?._binding;
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
            
            if (this._antiAliasing != this.queryAntiAliasing()) {
                return true;
            }

            var size = this.queryWindowSize();
            if (this._lastWindowWidth != size.x
                || this._lastWindowHeight != size.y) {
                return true;
            }

            if (this._viewMetricsChanged) {
                return true;
            }

            return false;
        }

        public virtual void OnGUI(Event evt = null) {
            evt = evt ?? Event.current;
            using (this.getScope()) {
                if (this.displayMetricsChanged()) {
                    this._devicePixelRatio = this.queryDevicePixelRatio();
                    this._antiAliasing = this.queryAntiAliasing();

                    var size = this.queryWindowSize();
                    this._lastWindowWidth = size.x;
                    this._lastWindowHeight = size.y;
                    this._physicalSize = new Size(
                        this._lastWindowWidth * this._devicePixelRatio,
                        this._lastWindowHeight * this._devicePixelRatio);

                    this.updateSafeArea();
                    this._viewMetricsChanged = false;
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

        protected abstract float queryDevicePixelRatio();
        protected abstract int queryAntiAliasing();
        protected abstract Vector2 queryWindowSize();

        protected virtual Surface createSurface() {
            return new WindowSurfaceImpl();
        }

        void _beginFrame() {
            if (this.onBeginFrame != null) {
                this.onBeginFrame(this.getTime());
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
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.up,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseDrag) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.move,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseMove) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.hover,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.ScrollWheel) {
                    this.onScroll(-evt.delta.x * this._devicePixelRatio,
                        -evt.delta.y * this._devicePixelRatio,
                        evt.mousePosition.x * this._devicePixelRatio,
                        evt.mousePosition.y * this._devicePixelRatio,
                        InputUtils.getScrollButtonKey()
                    );
                }
                else if (evt.type == EventType.DragUpdated) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.dragFromEditorMove,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * this._devicePixelRatio,
                        physicalY: evt.mousePosition.y * this._devicePixelRatio
                    );
                }
                else if (evt.type == EventType.DragPerform) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.dragFromEditorRelease,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
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

            RawKeyboard.instance._handleKeyEvent(Event.current);
            TextInput.OnGUI();
        }

        public void onScroll(float deltaX, float deltaY, float posX, float posY, int buttonId) {
            this._scrollInput.onScroll(deltaX,
                deltaY,
                posX,
                posY,
                buttonId
            );
        }

        void _updateScrollInput(float deltaTime) {
            var deltaScroll = this._scrollInput.getScrollDelta(deltaTime);

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
            if (this._physicalSize == null || this._physicalSize.isEmpty) {
                this.updatePhysicalSize();
            }

            this.updateDeltaTime();
            this.updateFPS(this.unscaledDeltaTime);

            Timer.update();

            bool hasFocus = this.hasFocus();
            using (this.getScope()) {
                WidgetsBinding.instance.focusManager.focusNone(!hasFocus);
                this._updateScrollInput(this.deltaTime);
                TextInput.Update();
                this._timerProvider.update(this.flushMicrotasks);
                this.flushMicrotasks();
            }
        }

        static readonly TimeSpan _coolDownDelay = new TimeSpan(0, 0, 0, 0, 200);
        static Timer frameCoolDownTimer;

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            if (regenerateLayerTree) {
                this._regenerateLayerTree = true;
            }

            onFrameRateSpeedUp();
            frameCoolDownTimer?.cancel();
            frameCoolDownTimer = instance.run(
                _coolDownDelay,
                () => {
                    onFrameRateCoolDown();
                    frameCoolDownTimer = null;
                });
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
            layerTree.antiAliasing = this._antiAliasing;
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

        public void attachRootWidget(Func<Widget> root) {
            using (this.getScope()) {
                this._binding.attachRootWidget(root());
            }
        }

        internal void _forceRepaint() {
            using (this.getScope()) {
                RenderObjectVisitor visitor = null;
                visitor = (child) => {
                    child.markNeedsPaint();
                    child.visitChildren(visitor);
                };
                this._binding.renderView?.visitChildren(visitor);
            }
        }
    }
}