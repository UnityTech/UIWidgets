using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RawImage = UnityEngine.UI.RawImage;
using Rect = UnityEngine.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.engine {
    public class UIWidgetWindowAdapter : WindowAdapter {
        readonly WidgetCanvas _widgetCanvas;
        bool _needsPaint;

        protected override void updateSafeArea() {
            this._padding = new WindowPadding(
                Screen.safeArea.x, 
                Screen.safeArea.y, 
                Screen.width - Screen.safeArea.width,
                Screen.height - Screen.safeArea.height);
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            this._needsPaint = true;
        }

        public UIWidgetWindowAdapter(WidgetCanvas widgetCanvas) {
            this._widgetCanvas = widgetCanvas;
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
            return new EditorWindowSurface(this._widgetCanvas.applyRenderTexture);
        }

        public override GUIContent titleContent {
            get { return new GUIContent(this._widgetCanvas.gameObject.name); }
        }

        protected override double queryDevicePixelRatio() {
            return this._widgetCanvas.devicePixelRatio;
        }

        protected override Vector2 queryWindowSize() {
            var size = this._widgetCanvas.rectTransform.rect.size;
            size = size * this._widgetCanvas.canvas.scaleFactor / (float)this._widgetCanvas.devicePixelRatio;
            return new Vector2	(Mathf.Round	(size.x), Mathf.Round	(size.y));
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class WidgetCanvas : RawImage, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler {
        static Event _repaintEvent;

        [SerializeField]
        protected double devicePixelRatioOverride;
        
        WindowAdapter _windowAdapter;
        Texture _texture;
        Vector2 _lastMouseMove;
        bool _mouseEntered;

        const int mouseButtonNum = 3;
        const int mouseScrollId = mouseButtonNum + 1;

        readonly ScrollInput _scrollInput = new ScrollInput();

        protected override void OnEnable() {
            base.OnEnable();

            if (_repaintEvent == null) {
                _repaintEvent = new Event {type = EventType.Repaint};
            }

            D.assert(this._windowAdapter == null);
            this._windowAdapter = new UIWidgetWindowAdapter(this);

            this._windowAdapter.OnEnable();
            var root = new WidgetsApp(
                home: this.getWidget(),
                window: this._windowAdapter,
                routes: this.routes,
                textStyle: this.textStyle,
                pageRouteBuilder: this.pageRouteBuilder,
                onGenerateRoute: this.onGenerateRoute,
                onUnknownRoute: this.onUnknownRoute);


            this._windowAdapter.attachRootWidget(root);
            this._lastMouseMove = Input.mousePosition;
        }

        public double devicePixelRatio {
            get { return this.devicePixelRatioOverride > 0 ? this.devicePixelRatioOverride : DisplayMetrics.devicePixelRatio; }
        }

        protected virtual Dictionary<string, WidgetBuilder> routes {
            get { return null; }
        }

        protected virtual string initialRoute {
            get { return null; }
        }

        protected virtual RouteFactory onGenerateRoute {
            get { return null; }
        }

        protected virtual RouteFactory onUnknownRoute {
            get { return null; }
        }

        protected virtual TextStyle textStyle {
            get { return null; }
        }

        protected virtual PageRouteFactory pageRouteBuilder {
            get {
                return (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<double> animation,
                            Animation<double> secondaryAnimation) => builder(context)
                    );
            }
        }

        protected override void OnDisable() {
            D.assert(this._windowAdapter != null);
            this._windowAdapter.OnDisable();
            this._windowAdapter = null;
            base.OnDisable();
        }

        protected virtual Widget getWidget() {
            return null;
        }

        internal void applyRenderTexture(Rect screenRect, Texture texture, Material mat) {
            this.texture = texture;
            this.material = mat;
        }

        void Update() {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != this.gameObject) {
                this.unfocusIfNeeded();
            }

            if (this._mouseEntered && (this._lastMouseMove.x != Input.mousePosition.x ||
                                       this._lastMouseMove.y != Input.mousePosition.y)) {
                this.handleMouseMove();
            }

            if (this._mouseEntered) {
                if (Input.mouseScrollDelta.y != 0 || Input.mouseScrollDelta.x != 0) {
                    var scaleFactor = this.canvas.scaleFactor;
                    var pos = this.getPointPosition(Input.mousePosition);
                    this._scrollInput.onScroll((float) (Input.mouseScrollDelta.x * scaleFactor),
                        (float) (Input.mouseScrollDelta.y * scaleFactor),
                        pos.x,
                        pos.y,
                        this.getScrollButton());
                }

                var deltaScroll = this._scrollInput.getScrollDelta();
                if (deltaScroll != Vector2.zero) {
                    this._windowAdapter.postPointerEvent(new ScrollData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.scroll,
                        kind: PointerDeviceKind.mouse,
                        device: this._scrollInput.getDeviceId(),
                        physicalX: this._scrollInput.getPointerPosX(),
                        physicalY: this._scrollInput.getPointerPosY(),
                        scrollX: deltaScroll.x,
                        scrollY: deltaScroll.y
                    ));
                }
            }


            this._lastMouseMove = Input.mousePosition;

            D.assert(this._windowAdapter != null);
            this._windowAdapter.Update();
            this._windowAdapter.OnGUI(_repaintEvent);
        }

        void OnGUI() {
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
                this._windowAdapter.OnGUI(Event.current);
            }
        }

        void handleMouseMove() {
            var pos = this.getPointPosition(Input.mousePosition);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: this.getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }

        int getScrollButton() {
            return mouseScrollId;
        }

        int getMouseButtonDown() {
            for (int key = 0; key < mouseButtonNum; key++) {
                if (Input.GetMouseButton(key)) {
                    return key;
                }
            }

            return 0;
        }

        public void OnPointerDown(PointerEventData eventData) {
            EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);
            var position = this.getPointPosition(eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.down,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerUp(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.up,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public Vector2 getPointPosition(PointerEventData eventData) {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, eventData.position,
                eventData.enterEventCamera, out localPoint);
            var scaleFactor = this.canvas.scaleFactor;
            localPoint.x = (localPoint.x - this.rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (this.rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        public Vector2 getPointPosition(Vector2 position) {
            Vector2 localPoint;
            Camera eventCamera = null;

            if (this.canvas.renderMode != RenderMode.ScreenSpaceCamera) {
                eventCamera = this.canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }


            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, position,
                eventCamera, out localPoint);
            var scaleFactor = this.canvas.scaleFactor;
            localPoint.x = (localPoint.x - this.rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (this.rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        public void OnDrag(PointerEventData eventData) {
            var position = this.getPointPosition(eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.move,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerEnter(PointerEventData eventData) {
            this._mouseEntered = true;
            this._lastMouseMove = eventData.position;
            var position = this.getPointPosition(eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerExit(PointerEventData eventData) {
            this._mouseEntered = false;
            var position = this.getPointPosition(eventData);
            this._windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        void unfocusIfNeeded() {
            using (this._windowAdapter.getScope()) {
                var focusNode = WidgetsBinding.instance.focusManager.currentFocus;
                if (focusNode != null) {
                    focusNode.unfocus();
                }
            }
        }
    }
}