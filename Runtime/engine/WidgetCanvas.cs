using System;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.engine
{
    public class UIWidgetWindowAdapter : WindowAdapter
    {
        private WidgetCanvas _widgetCanvas;

        public UIWidgetWindowAdapter(WidgetCanvas widgetCanvas)
        {
            this._widgetCanvas = widgetCanvas;
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
    public abstract class WidgetCanvas : MaskableGraphic, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        private WindowAdapter _windowAdapter;
        private PaintingBinding _paintingBinding;
        private Texture _texture;
        private Vector2 _lastMouseMove;
        private bool _mouseEntered;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_windowAdapter == null)
            {
                this._paintingBinding = new PaintingBinding(null);
                _paintingBinding.initInstances();
                _windowAdapter = new UIWidgetWindowAdapter(this);
            }
            
            _windowAdapter.OnEnable();
            var root = new WidgetsApp(null, getWidget());
            _windowAdapter.attachRootWidget(root);
            
            _lastMouseMove = Input.mousePosition;
        }

        private void OnDisable()
        {
            this._windowAdapter.OnDisable();
            base.OnDisable();
        }

        protected abstract Widget getWidget();

        public override Texture mainTexture
        {
            get { return _texture; }
        }

        internal void applyRenderTexture(Rect screenRect, Texture texture, Material mat)
        {
            _texture = texture;
            SetMaterialDirty();
        }

        private void OnDestroy()
        {
            base.OnDestroy();
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
            if (this._windowAdapter != null)
            {
                this._windowAdapter.Update();
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                this._windowAdapter.OnGUI();
            }

            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp)
            {
                if (this._windowAdapter != null)
                {
                    this._windowAdapter.OnGUI();
                }
            }
        }

        private void OnMouseOver()
        {
           
            var pos = getPointPosition(Input.mousePosition);
            // Debug.Log(string.Format("mouse move {0} {1}", pos.x, pos.y));
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
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
                timeStamp: DateTime.Now,
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
                timeStamp: DateTime.Now,
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
            // Debug.Log("mouse posse " + position.x + " " + position.y);
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
                timeStamp: DateTime.Now,
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
                timeStamp: DateTime.Now,
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
                timeStamp: DateTime.Now,
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