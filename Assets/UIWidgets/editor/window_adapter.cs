using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIWidgets.async;
using UIWidgets.flow;
using UIWidgets.rendering;
using UIWidgets.service;
using UIWidgets.ui;
using UIWidgets.widgets;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Rect = UnityEngine.Rect;

namespace UIWidgets.editor
{
    
    public abstract class WindowAdapter : Window {
        
        private static List<WindowAdapter> _windowAdapters = new List<WindowAdapter>();
        private bool _alive;
        public static IEnumerable<WindowAdapter> windowAdapters
        {
            get { return _windowAdapters; }
        }

        public WindowAdapter(Rect position, double devicePixelRatio)
        {
            this._alive = true;
            this._lastPosition = position;

            this._devicePixelRatio = devicePixelRatio;
            this._physicalSize = new Size(
                this._lastPosition.width * devicePixelRatio,
                this._lastPosition.height * devicePixelRatio);

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

        public WidgetInspectorService widgetInspectorService
        {
            get { return _binding.widgetInspectorService; }
        }

        public virtual GUIContent titleContent
        {
            get { return null; }
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

        public void PostPointerEvent(List<PointerData> data)
        {
            WithBinding(() =>
            {
                this.onPointerEvent(new PointerDataPacket(data));
            });
        }
        
        public void PostPointerEvent(PointerData data)
        {
            PostPointerEvent(new List<PointerData>(){data});
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
                   
                        var pysicalPos = convertPointerPosition(evt.mousePosition);
                        Debug.Log("clicked");
                        pointerData = new PointerData(
                            timeStamp: DateTime.Now,
                            change: PointerChange.down,
                            kind: PointerDeviceKind.mouse,
                            device: evt.button,
                            physicalX: pysicalPos.x,
                            physicalY: pysicalPos.y
                        );            
                } else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                    var pysicalPos = convertPointerPosition(evt.mousePosition);
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.up,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: pysicalPos.x,
                        physicalY: pysicalPos.y
                    );
                } else if (evt.type == EventType.MouseDrag) {
                    var pysicalPos = convertPointerPosition(evt.mousePosition);
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.move,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: pysicalPos.x,
                        physicalY: pysicalPos.y
                    );
                } else if (evt.type == EventType.MouseMove)
                {
                    var pysicalPos = convertPointerPosition(evt.mousePosition);
                    pointerData = new PointerData(
                        timeStamp: DateTime.Now,
                        change: PointerChange.hover,
                        kind: PointerDeviceKind.mouse,
                        device: evt.button,
                        physicalX: pysicalPos.x,
                        physicalY: pysicalPos.y
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

        protected abstract void getWindowMetrics(out double  devicePixelRatio , out Rect position);

        protected abstract Vector2d convertPointerPosition(Vector2 postion);
        
        private void doUpdate() {
            this.flushMicrotasks();

            this._timerProvider.update();
            double devicePixelRatio;
            Rect newPosition;
            getWindowMetrics(out devicePixelRatio, out newPosition);
            bool dirty = false;
            if (this._devicePixelRatio != devicePixelRatio) {
                dirty = true;
            }

            if (this._lastPosition != newPosition) {
                dirty = true;
            }

            if (dirty) {
                this._devicePixelRatio = devicePixelRatio;
                this._lastPosition = newPosition;
                this._physicalSize = new Size(
                    this._lastPosition.width * devicePixelRatio,
                    this._lastPosition.height * devicePixelRatio);

                if (this.onMetricsChanged != null) {
                    this.onMetricsChanged();
                }
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