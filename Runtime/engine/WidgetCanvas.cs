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

namespace Unity.UIWidgets.engine
{
    public class UIWidgetWindowAdapter : WindowAdapter
    {
        private WidgetCanvas _widgetCanvas;
        private bool _needsPaint;

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            _needsPaint = true;
        }

        public UIWidgetWindowAdapter(WidgetCanvas widgetCanvas)
        {
            this._widgetCanvas = widgetCanvas;
        }


        public override void OnGUI(Event evt)
        {
            if (evt.type == EventType.Repaint)
            {
                if (!_needsPaint)
                {
                    return;
                }
                _needsPaint = false;
            }
            base.OnGUI(evt);
        }

        protected override Surface createSurface()
        {
            return new EditorWindowSurface(_widgetCanvas.applyRenderTexture);
        }

        public override GUIContent titleContent
        {
            get { return new GUIContent(_widgetCanvas.gameObject.name); }
        }

        protected override double queryDevicePixelRatio()
        {
            return _widgetCanvas.canvas.scaleFactor;
        }

        protected override Vector2 queryWindowSize()
        {
            return _widgetCanvas.rectTransform.rect.size;
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class WidgetCanvas : RawImage, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        private static Event _repaintEvent;
        
        private WindowAdapter _windowAdapter;
        private Texture _texture;
        private Vector2 _lastMouseMove;
        private bool _mouseEntered;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (_repaintEvent == null) {
                _repaintEvent = new Event {type = EventType.Repaint};
            }

            D.assert(this._windowAdapter == null);
            _windowAdapter = new UIWidgetWindowAdapter(this);
            
            _windowAdapter.OnEnable();
            var root = new WidgetsApp(home: getWidget(), window: _windowAdapter);
            _windowAdapter.attachRootWidget(root);
            _lastMouseMove = Input.mousePosition;
        }

        protected override void OnDisable()
        {
            D.assert(this._windowAdapter != null);
            this._windowAdapter.OnDisable();
            this._windowAdapter = null;
            base.OnDisable();
        }

        protected abstract Widget getWidget();

        internal void applyRenderTexture(Rect screenRect, Texture texture, Material mat) {
            this.texture = texture;
            this.material = mat;
        }

        private void Update()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
            {
                unfocusIfNeeded();
            }
            if (_mouseEntered && (_lastMouseMove.x != Input.mousePosition.x || _lastMouseMove.y != Input.mousePosition.y))
            {
                this.OnMouseOver();
            }

            _lastMouseMove = Input.mousePosition;
            
            D.assert(this._windowAdapter != null);
            this._windowAdapter.Update();
            this._windowAdapter.OnGUI(_repaintEvent);
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp)
            {
                this._windowAdapter.OnGUI(Event.current);
            }
        }

        private void OnMouseOver()
        {
            var pos = getPointPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }

        private int getMouseButtonDown()
        {
            for (int key = 0; key < 3; key++)
            {
                if (Input.GetMouseButton(key))
                {
                    return key;
                }
            }
            return 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            var position = getPointPosition(eventData);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.down,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var position = getPointPosition(eventData);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.up,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public Vector2 getPointPosition(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                eventData.enterEventCamera, out localPoint);
            localPoint.x = localPoint.x - rectTransform.rect.min.x;
            localPoint.y = rectTransform.rect.max.y - localPoint.y;
            return localPoint;
        }

        public Vector2 getPointPosition(Vector2 position)
        { 
            Vector2 localPoint;
            Camera eventCamera = null;

            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                eventCamera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }


            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position,
                eventCamera, out localPoint);
            localPoint.x = localPoint.x - rectTransform.rect.min.x;
            localPoint.y = rectTransform.rect.max.y - localPoint.y;
            return localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = getPointPosition(eventData);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.move,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseEntered = true;
            _lastMouseMove = eventData.position;
            var position = getPointPosition(eventData);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseEntered = false;
            var position = getPointPosition(eventData);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: (int) eventData.button,
                physicalX: position.x,
                physicalY: position.y
            ));
        }

        private void unfocusIfNeeded()
        {
            using (_windowAdapter.getScope())
            {
                var focusNode = WidgetsBinding.instance.focusManager.currentFocus;
                if (focusNode != null)
                {
                    focusNode.unfocus();
                }
            }
        }
    }
}