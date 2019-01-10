using System.Collections.Generic;
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

namespace Unity.UIWidgets.engine {
    public class UIWidgetWindowAdapter : WindowAdapter {
        readonly WidgetCanvas _widgetCanvas;
        bool _needsPaint;

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            this._needsPaint = true;
        }

        public UIWidgetWindowAdapter(WidgetCanvas widgetCanvas) {
            this._widgetCanvas = widgetCanvas;
        }


        public override void OnGUI(Event evt) {
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
            return this._widgetCanvas.canvas.scaleFactor;
        }

        protected override Vector2 queryWindowSize() {
            return this._widgetCanvas.rectTransform.rect.size;
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class WidgetCanvas : RawImage, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler {
        static Event _repaintEvent;

        WindowAdapter _windowAdapter;
        Texture _texture;
        Vector2 _lastMouseMove;
        bool _mouseEntered;

        protected override void OnEnable() {
            base.OnEnable();

            if (_repaintEvent == null) {
                _repaintEvent = new Event {type = EventType.Repaint};
            }

            D.assert(this._windowAdapter == null);
            this._windowAdapter = new UIWidgetWindowAdapter(this);

            this._windowAdapter.OnEnable();
            var root = new WidgetsApp(home: this.getWidget(),
                window: this._windowAdapter,
                routes: this.routes,
                pageRouteBuilder: this.pageRouteBuilder,
                onGenerateRoute: this.onGenerateRoute,
                onUnknownRoute: this.onUnknownRoute);
            
                  
            this._windowAdapter.attachRootWidget(root);
            this._lastMouseMove = Input.mousePosition;
        }

        protected virtual Dictionary<string, WidgetBuilder> routes => null;

        protected virtual string initialRoute => null;
        protected virtual RouteFactory onGenerateRoute => null;
        
        protected virtual RouteFactory onUnknownRoute => null;
        
        protected virtual PageRouteFactory pageRouteBuilder => (RouteSettings settings, WidgetBuilder builder) =>
            new PageRouteBuilder(
                settings: settings,
                pageBuilder: (BuildContext context, Unity.UIWidgets.animation.Animation<double> animation, 
                    Unity.UIWidgets.animation.Animation<double> secondaryAnimation) => builder(context)
            );
        
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
                this.OnMouseOver();
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

        void OnMouseOver() {
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

        int getMouseButtonDown() {
            for (int key = 0; key < 3; key++) {
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
            localPoint.x = localPoint.x - this.rectTransform.rect.min.x;
            localPoint.y = this.rectTransform.rect.max.y - localPoint.y;
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
            localPoint.x = localPoint.x - this.rectTransform.rect.min.x;
            localPoint.y = this.rectTransform.rect.max.y - localPoint.y;
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
