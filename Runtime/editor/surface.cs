using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.editor {
    public delegate bool SubmitCallback(SurfaceFrame surfaceFrame, Canvas canvas);

    public class SurfaceFrame : IDisposable {
        bool _submitted;

        readonly GrSurface _surface;

        readonly SubmitCallback _submitCallback;

        public SurfaceFrame(GrSurface surface, SubmitCallback submitCallback) {
            this._surface = surface;
            this._submitCallback = submitCallback;
        }

        public void Dispose() {
            if (this._submitCallback != null && !this._submitted) {
                this._submitCallback(this, null);
            }
        }

        public Canvas getCanvas() {
            return this._surface != null ? this._surface.getCanvas() : null;
        }

        public bool submit() {
            if (this._submitted) {
                return false;
            }

            this._submitted = this._performSubmit();

            return this._submitted;
        }

        bool _performSubmit() {
            if (this._submitCallback == null) {
                return false;
            }

            if (this._submitCallback(this, this.getCanvas())) {
                return true;
            }

            return false;
        }
    }

    public interface Surface : IDisposable {
        SurfaceFrame acquireFrame(Size size, float devicePixelRatio, int antiAliasing);

        MeshPool getMeshPool();
    }

    public class WindowSurfaceImpl : Surface {
        static Material _guiTextureMat;
        static Material _uiDefaultMat;

        public delegate void DrawToTargetFunc(Rect screenRect, Texture texture, Material mat);

        internal static Material _getGUITextureMat() {
            if (_guiTextureMat) {
                return _guiTextureMat;
            }

            var guiTextureShader = Shader.Find("UIWidgets/GUITexture");
            if (guiTextureShader == null) {
                throw new Exception("UIWidgets/GUITexture not found");
            }

            _guiTextureMat = new Material(guiTextureShader);
            _guiTextureMat.hideFlags = HideFlags.HideAndDontSave;
            return _guiTextureMat;
        }

        internal static Material _getUIDefaultMat() {
            if (_uiDefaultMat) {
                return _uiDefaultMat;
            }

            var uiDefaultShader = Shader.Find("UIWidgets/UIDefault");
            if (uiDefaultShader == null) {
                throw new Exception("UIWidgets/UIDefault not found");
            }

            _uiDefaultMat = new Material(uiDefaultShader);
            _uiDefaultMat.hideFlags = HideFlags.HideAndDontSave;
            return _uiDefaultMat;
        }


        GrSurface _surface;
        readonly DrawToTargetFunc _drawToTargetFunc;
        MeshPool _meshPool = new MeshPool();

        public WindowSurfaceImpl(DrawToTargetFunc drawToTargetFunc = null) {
            this._drawToTargetFunc = drawToTargetFunc;
        }

        public SurfaceFrame acquireFrame(Size size, float devicePixelRatio, int antiAliasing) {
            this._createOrUpdateRenderTexture(size, devicePixelRatio, antiAliasing);

            return new SurfaceFrame(this._surface,
                (frame, canvas) => this._presentSurface(canvas));
        }

        public MeshPool getMeshPool() {
            return this._meshPool;
        }

        public void Dispose() {
            if (this._surface != null) {
                this._surface.Dispose();
                this._surface = null;
            }

            if (this._meshPool != null) {
                this._meshPool.Dispose();
                this._meshPool = null;
            }
        }

        protected bool _presentSurface(Canvas canvas) {
            if (canvas == null) {
                return false;
            }

            this._surface.getCanvas().flush();
            this._surface.getCanvas().reset();

            var screenRect = new Rect(0, 0,
                this._surface.size.width / this._surface.devicePixelRatio,
                this._surface.size.height / this._surface.devicePixelRatio);

            if (this._drawToTargetFunc == null) {
                Graphics.DrawTexture(screenRect, this._surface.getRenderTexture(),
                    _getGUITextureMat());
            }
            else {
                this._drawToTargetFunc(screenRect, this._surface.getRenderTexture(),
                    _getUIDefaultMat());
            }

            return true;
        }

        void _createOrUpdateRenderTexture(Size size, float devicePixelRatio, int antiAliasing) {
            if (this._surface != null
                && this._surface.size == size
                && this._surface.devicePixelRatio == devicePixelRatio
                && this._surface.antiAliasing == antiAliasing
                && this._surface.getRenderTexture() != null) {                
                return;
            }

            if (this._surface != null) {
                this._surface.Dispose();
                this._surface = null;
            }

            this._surface = new GrSurface(size, devicePixelRatio, antiAliasing, this._meshPool);
        }
    }

    public class GrSurface : IDisposable {
        public readonly Size size;

        public readonly float devicePixelRatio;
        
        public readonly int antiAliasing;

        readonly MeshPool _meshPool;

        RenderTexture _renderTexture;

        CommandBufferCanvas _canvas;

        public RenderTexture getRenderTexture() {
            return this._renderTexture;
        }

        public Canvas getCanvas() {
            if (this._canvas == null) {
                this._canvas = new CommandBufferCanvas(
                    this._renderTexture, this.devicePixelRatio, this._meshPool);
            }

            return this._canvas;
        }

        public GrSurface(Size size, float devicePixelRatio, int antiAliasing, MeshPool meshPool) {
            this.size = size;
            this.devicePixelRatio = devicePixelRatio;
            this.antiAliasing = antiAliasing;

            var desc = new RenderTextureDescriptor(
                (int) this.size.width, (int) this.size.height,
                RenderTextureFormat.Default, 24) {
                useMipMap = false,
                autoGenerateMips = false,
            };
            
            if (antiAliasing != 0) {
                desc.msaaSamples = antiAliasing;
            }

            this._renderTexture = new RenderTexture(desc);
            this._renderTexture.hideFlags = HideFlags.HideAndDontSave;

            this._meshPool = meshPool;
        }

        public void Dispose() {
            if (this._renderTexture) {
                this._renderTexture = ObjectUtils.SafeDestroy(this._renderTexture);
            }

            if (this._canvas != null) {
                this._canvas.reset();
                this._canvas.dispose();
                this._canvas = null;
            }
        }
    }
}