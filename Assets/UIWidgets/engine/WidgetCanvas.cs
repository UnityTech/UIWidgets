using System;
using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.ui;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.engine
{
    
    public class WidgetCanvas: MonoBehaviour
    {
        
        [SerializeField]
        private int _canvasWidth = 1000;
        
        [SerializeField]
        private int _canvasHeight = 800;
        
        private WindowAdapter _windowAdapter;
        private PaintingBinding _paintingBinding;
        private RenderTexture _renderTexture;
        
        void OnEnable()
        {
            if (_windowAdapter == null)
            {
                this._paintingBinding = new PaintingBinding(null);
                _paintingBinding.initInstances();
                _windowAdapter = new CanvasWindowAdapter(this);
                var root = new WidgetsApp(null, getWidget());
                _windowAdapter.attachRootWidget(root);
            }

            setupMeshRenderer();
            var boxColider = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
            boxColider.size = new Vector3(1, 1, 0.00001f); // very thin box
        }

        protected virtual Widget getWidget()
        {
            return new AsScreen();
        }

        private  void setupMeshRenderer()
        {
            var meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            var shader = Shader.Find("UI/Default"); // todo
            var material = new Material(shader);
            meshRenderer.material = material;
            
            var meshFilter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            
            var mesh = new Mesh();
            meshFilter.mesh = mesh;
    
            
            var vertices = new Vector3[4];

            float minX = -0.5f;
            float minY = -0.5f;
            float maxX = 0.5f;
            float maxY = 0.5f;
            vertices[0] = new Vector3(minX, minY, 0);
            vertices[1] = new Vector3(maxX, minY, 0);
            vertices[2] = new Vector3(minX, maxY, 0);
            vertices[3] = new Vector3(maxX, maxY, 0);
    
            mesh.vertices = vertices;
    
            var tri  = new int[6];

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;
    
            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;
    
            mesh.triangles = tri;
    
            var uv = new Vector2[4];
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);
            mesh.uv = uv;
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                _renderTexture = null;
            }
            Destroy(GetComponent<MeshRenderer>());
            Destroy(GetComponent<MeshFilter>());
            Destroy(GetComponent<BoxCollider>());
        }
        
        private void Update() {
            if (this._windowAdapter != null) {
                this._windowAdapter.Update();
            }
        }

        private void ensureRenderTexture()
        {
            if (_renderTexture != null && _renderTexture.IsCreated() &&
                _renderTexture.width == getCanvasWidth() && _renderTexture.height == getCanvasHeight())
            {
                return;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }
            _renderTexture = new RenderTexture(getCanvasWidth(), getCanvasHeight(), 24);
        }

        
        private void OnGUI()
        {
            var effectiveWidth = getCanvasWidth();
            var effectiveHeight = getCanvasHeight();
            if (Event.current.type == EventType.Repaint)
            {
                GL.PushMatrix();
                GL.LoadIdentity();
                Matrix4x4 m =Matrix4x4.Scale(new Vector3(2.0f / effectiveWidth, -2.0f / effectiveHeight, 1.0f))
                             * Matrix4x4.Translate(new Vector3(-effectiveWidth / 2.0f, -effectiveHeight / 2.0f, 0));
                GL.LoadProjectionMatrix(m);

                ensureRenderTexture();
                var renderer = GetComponent<MeshRenderer>();
                renderer.material.mainTexture = _renderTexture;
                RenderTexture currentActiveRT = RenderTexture.active;
                try
                {
                    RenderTexture.active = _renderTexture;
                    GL.Clear(true, true, new UnityEngine.Color(0,0,0,0));
                    this._windowAdapter.OnGUI();
                }
                finally
                {
                    RenderTexture.active = currentActiveRT;
                }
                
                GL.PopMatrix();
                return;
           
            }
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp)
            {
                if (this._windowAdapter != null) {
                    this._windowAdapter.OnGUI();
                }
            }
        }

        void OnMouseEnter()
        {
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }

        void OnMouseExit()
        {
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }
        
        void OnMouseOver()
        {
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }

        void OnMouseDown()
        {
            int x = Screen.width;
            int y = Screen.height;
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                    timeStamp: DateTime.Now,
                    change: PointerChange.down,
                    kind: PointerDeviceKind.mouse,
                    device: getMouseButtonDown(),
                    physicalX: pos.x,
                    physicalY: pos.y
                ));
        }

        void OnMouseUp()
        {
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
                change: PointerChange.up,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }

        void OnMouseDrag()
        {
            var pos = convertPosition(Input.mousePosition);
            this._windowAdapter.PostPointerEvent(new PointerData(
                timeStamp: DateTime.Now,
                change: PointerChange.move,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.x,
                physicalY: pos.y
            ));
        }
        
        Vector2d convertPosition(Vector2 mousePos)
        {
            var cam = Camera.main;
            var plan = new Plane(transform.TransformDirection(0, 0, 1), transform.TransformPoint(0, 0, 0));
            var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
            float enter;
            Vector3 hitPoint;
            if (plan.Raycast(ray, out enter))
            {
                hitPoint = ray.GetPoint(enter);
            }
            else
            {
                ray.direction = -ray.direction;
                plan.Raycast(ray, out enter);
                hitPoint = ray.GetPoint(enter);
            }

            var localPoint = transform.InverseTransformPoint(hitPoint);
            return new Vector2d(
                (localPoint.x + 0.5) * getCanvasWidth() * _windowAdapter.devicePixelRatio,
                (-localPoint.y + 0.5) * getCanvasHeight() * _windowAdapter.devicePixelRatio
            );
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

        private int getCanvasWidth()
        {
            return _canvasWidth;
        }
        
        private int getCanvasHeight()
        {
            return _canvasHeight;
        }

        public Vector2 size
        {
            get
            {
                return new Vector2(_canvasWidth, _canvasHeight);
            }
        }

        public double devicePixelRatio
        {
            get { return 1; }
        }
        
        public void scheduleFrame()
        {
        }

        public GUIContent titleContent
        {
            get
            {
                return new GUIContent(gameObject.name);
            }
        }
    }
}