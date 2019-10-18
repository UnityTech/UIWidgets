using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RawImage = UnityEngine.UI.RawImage;
using Rect = UnityEngine.Rect;
using Texture = UnityEngine.Texture;

namespace Unity.UIWidgets.engine {
    public class UIWidgetWindowAdapter : WindowAdapter {
        readonly UIWidgetsPanel _uiWidgetsPanel;
        bool _needsPaint;


        protected override void updateSafeArea() {
            this._padding = this._uiWidgetsPanel.viewPadding;
            this._viewInsets = this._uiWidgetsPanel.viewInsets;
        }

        protected override bool hasFocus() {
            return EventSystem.current != null &&
                   EventSystem.current.currentSelectedGameObject == this._uiWidgetsPanel.gameObject;
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            this._needsPaint = true;
        }

        public UIWidgetWindowAdapter(UIWidgetsPanel uiWidgetsPanel) {
            this._uiWidgetsPanel = uiWidgetsPanel;
        }


        public override void OnGUI(Event evt) {
            if (this.displayMetricsChanged()) {
                this._needsPaint = true;
            }

            if (evt.type == EventType.Repaint) {
                if (!this._needsPaint) {
                    return;
                }

                this._needsPaint = false;
            }

            base.OnGUI(evt);
        }

        protected override Surface createSurface() {
            return new WindowSurfaceImpl(this._uiWidgetsPanel.applyRenderTexture);
        }

        public override GUIContent titleContent {
            get { return new GUIContent(this._uiWidgetsPanel.gameObject.name); }
        }

        protected override float queryDevicePixelRatio() {
            return this._uiWidgetsPanel.devicePixelRatio;
        }
        
        protected override int queryAntiAliasing() {
            return this._uiWidgetsPanel.antiAliasing;
        }

        protected override Vector2 queryWindowSize() {
            var rect = this._uiWidgetsPanel.rectTransform.rect;
            // Here we use ReferenceEquals instead of "==" due to
            // https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // In short, "==" is overloaded for UnityEngine.Object and will bring performance issues
            if (!ReferenceEquals(this._uiWidgetsPanel.canvas, null)) {
                var size = new Vector2(rect.width, rect.height) *
                           this._uiWidgetsPanel.canvas.scaleFactor / this._uiWidgetsPanel.devicePixelRatio;
                size.x = Mathf.Round(size.x);
                size.y = Mathf.Round(size.y);
                return size;
            }

            return new Vector2(0, 0);
        }

        public Offset windowPosToScreenPos(Offset windowPos) {
            Camera camera = null;
            var canvas = this._uiWidgetsPanel.canvas;
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera) {
                camera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }

            var pos = new Vector2(windowPos.dx, windowPos.dy);
            pos = pos * this.queryDevicePixelRatio() / this._uiWidgetsPanel.canvas.scaleFactor;
            var rectTransform = this._uiWidgetsPanel.rectTransform;
            var rect = rectTransform.rect;
            pos.x += rect.min.x;
            pos.y = rect.max.y - pos.y;
            var worldPos = rectTransform.TransformPoint(new Vector2(pos.x, pos.y));
            var screenPos = RectTransformUtility.WorldToScreenPoint(camera, worldPos);
            return new Offset(screenPos.x, Screen.height - screenPos.y);
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsPanel : RawImage, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler, WindowHost {
        static Event _repaintEvent;

        [Tooltip("set to zero if you want to use the default device pixel ratio of the target platforms; otherwise the " +
                 "device pixel ratio will be forced to the given value on all devices.")]
        [SerializeField] protected float devicePixelRatioOverride;
        
        [Tooltip("set to true will enable the hardware anti-alias feature, which will improve the appearance of the UI greatly but " +
                 "making it much slower. Enable it only when seriously required.")]
        [SerializeField] protected bool hardwareAntiAliasing = false;
        WindowAdapter _windowAdapter;
        Texture _texture;
        Vector2 _lastMouseMove;

        readonly HashSet<int> _enteredPointers = new HashSet<int>();

        bool _viewMetricsCallbackRegistered;

        bool _mouseEntered {
            get { return !this._enteredPointers.isEmpty(); }
        }

        DisplayMetrics _displayMetrics;

        const int mouseButtonNum = 3;

        void _handleViewMetricsChanged(string method, List<JSONNode> args) {
            this._windowAdapter.onViewMetricsChanged();
            this._displayMetrics.onViewMetricsChanged();
        }

        protected override void OnEnable() {
            base.OnEnable();

            //Disable the default touch -> mouse event conversion on mobile devices
            Input.simulateMouseWithTouches = false;

            this._displayMetrics = DisplayMetricsProvider.provider();
            this._displayMetrics.OnEnable();

            this._enteredPointers.Clear();

            if (_repaintEvent == null) {
                _repaintEvent = new Event {type = EventType.Repaint};
            }

            D.assert(this._windowAdapter == null);
            this._windowAdapter = new UIWidgetWindowAdapter(this);

            this._windowAdapter.OnEnable();

            Widget root;
            using (this._windowAdapter.getScope()) {
                root = this.createWidget();
            }

            this._windowAdapter.attachRootWidget(root);
            this._lastMouseMove = Input.mousePosition;
        }

        public float devicePixelRatio {
            get {
                return this.devicePixelRatioOverride > 0
                    ? this.devicePixelRatioOverride
                    : this._displayMetrics.devicePixelRatio;
            }
        }
        
        public int antiAliasing {
            get { return this.hardwareAntiAliasing ? Window.defaultAntiAliasing : 0; }
        }

        public WindowPadding viewPadding {
            get { return this._displayMetrics.viewPadding; }
        }

        public WindowPadding viewInsets {
            get { return this._displayMetrics.viewInsets; }
        }

        protected override void OnDisable() {
            D.assert(this._windowAdapter != null);
            this._windowAdapter.OnDisable();
            this._windowAdapter = null;
            base.OnDisable();
        }

        protected virtual Widget createWidget() {
            return null;
        }

        public void recreateWidget() {
            Widget root;
            using (this._windowAdapter.getScope()) {
                root = this.createWidget();
            }

            this._windowAdapter.attachRootWidget(root);
        }

        internal void applyRenderTexture(Rect screenRect, Texture texture, Material mat) {
            this.texture = texture;
            this.material = mat;
        }

        protected virtual void Update() {
            this._displayMetrics.Update();
            UIWidgetsMessageManager.ensureUIWidgetsMessageManagerIfNeeded();

#if UNITY_ANDROID
            if (Input.GetKeyDown(KeyCode.Escape)) {
                this._windowAdapter.withBinding(() => { WidgetsBinding.instance.handlePopRoute(); });
            }
#endif

            if (!this._viewMetricsCallbackRegistered) {
                this._viewMetricsCallbackRegistered = true;
                UIWidgetsMessageManager.instance?.AddChannelMessageDelegate("ViewportMatricsChanged",
                    this._handleViewMetricsChanged);
            }

            if (this._mouseEntered) {
                if (this._lastMouseMove.x != Input.mousePosition.x || this._lastMouseMove.y != Input.mousePosition.y) {
                    this.handleMouseMovement();
                }
            }

            this._lastMouseMove = Input.mousePosition;

            if (this._mouseEntered) {
                this.handleMouseScroll();
            }

            D.assert(this._windowAdapter != null);
            this._windowAdapter.Update();
            this._windowAdapter.OnGUI(_repaintEvent);
        }

        void OnGUI() {
            this._displayMetrics.OnGUI();
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
                this._windowAdapter.OnGUI(Event.current);
            }
        }

        void handleMouseMovement() {
            var pos = this.getPointPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: this.getMouseButtonDown(),
                physicalX: pos.Value.x,
                physicalY: pos.Value.y
            ));
        }

        void handleMouseScroll() {
            if (Input.mouseScrollDelta.y != 0 || Input.mouseScrollDelta.x != 0) {
                var scaleFactor = this.canvas.scaleFactor;
                var pos = this.getPointPosition(Input.mousePosition);
                if (pos == null) {
                    return;
                }
                this._windowAdapter.onScroll(Input.mouseScrollDelta.x * scaleFactor,
                    Input.mouseScrollDelta.y * scaleFactor,
                    pos.Value.x,
                    pos.Value.y,
                    InputUtils.getScrollButtonKey());
            }
        }

        int getMouseButtonDown() {
            //default mouse button key = left mouse button
            var defaultKey = 0;
            for (int key = 0; key < mouseButtonNum; key++) {
                if (Input.GetMouseButton(key)) {
                    defaultKey = key;
                    break;
                }
            }

            return InputUtils.getMouseButtonKey(defaultKey);
        }

        public void OnPointerDown(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            if (position == null) {
                return;
            }
            EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.down,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerUp(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            if (position == null) {
                return;
            }
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.up,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        Camera getActiveCamera() {
            //refer to: https://zhuanlan.zhihu.com/p/37127981
            Camera eventCamera = null;
            if (this.canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
                eventCamera = this.canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }
            return eventCamera;
        }

        Vector2? getPointPosition(PointerEventData eventData) {
            Camera camera = this.getActiveCamera();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, eventData.position,
                camera, out localPoint);
            var scaleFactor = this.canvas.scaleFactor;
            localPoint.x = (localPoint.x - this.rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (this.rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        Vector2? getPointPosition(Vector2 position) {
            Vector2 localPoint;
            Camera eventCamera = this.getActiveCamera();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, position,
                eventCamera, out localPoint);
            var scaleFactor = this.canvas.scaleFactor;
            localPoint.x = (localPoint.x - this.rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (this.rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        public void OnDrag(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            if (position == null) {
                return;
            }
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.move,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerEnter(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            if (position == null) {
                return;
            }
            var pointerKey = InputUtils.getPointerDeviceKey(eventData);
            this._enteredPointers.Add(pointerKey);

            this._lastMouseMove = eventData.position;
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: pointerKey,
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerExit(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            if (position == null) {
                return;
            }
            var pointerKey = InputUtils.getPointerDeviceKey(eventData);
            this._enteredPointers.Remove(pointerKey);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: pointerKey,
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public Window window {
            get { return this._windowAdapter; }
        }
    }
}