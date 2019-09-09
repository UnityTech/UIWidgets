using System;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    enum InitStage {
        NotPrepared,
        Prepared,
        Ready
    }

    static partial class CanvasShader {
        static InitStage initStage = InitStage.NotPrepared;
        static int initialFrameCount;
        static Shader testShader;
        const string testShaderName = "UIWidgets/canvas_convexFill_cb";
        
        
        const bool enableComputeBuffer = true;

        public static bool supportComputeBuffer;

        static void DoPrepareDefaultShader() {
            supportComputeBuffer = false;
            
            var convexFillShader = GetShader("UIWidgets/canvas_convexFill");
            var fill0Shader = GetShader("UIWidgets/canvas_fill0");
            var fill1Shader = GetShader("UIWidgets/canvas_fill1");
            var stroke0Shader = GetShader("UIWidgets/canvas_stroke0");
            var stroke1Shader = GetShader("UIWidgets/canvas_stroke1");
            var texShader = GetShader("UIWidgets/canvas_tex");
            var stencilShader = GetShader("UIWidgets/canvas_stencil");
            var filterShader = GetShader("UIWidgets/canvas_filter");
            var shadowBoxShader = GetShader("UIWidgets/ShadowBox");
            var shadowRBoxShader = GetShader("UIWidgets/ShadowRBox");
            var strokeAlphaShader = GetShader("UIWidgets/canvas_strokeAlpha");

            _convexFillMat = new MaterialByBlendModeStencilComp(convexFillShader);
            _fill0Mat = new MaterialByStencilComp(fill0Shader);
            _fill1Mat = new MaterialByBlendMode(fill1Shader);
            _stroke0Mat = new MaterialByBlendModeStencilComp(stroke0Shader);
            _stroke1Mat = new Material(stroke1Shader) {hideFlags = HideFlags.HideAndDontSave};
            _strokeAlphaMat = new MaterialByBlendModeStencilComp(strokeAlphaShader);
            _texMat = new MaterialByBlendModeStencilComp(texShader);
            _stencilMat = new Material(stencilShader) {hideFlags = HideFlags.HideAndDontSave};
            _filterMat = new Material(filterShader) {hideFlags = HideFlags.HideAndDontSave};
            _shadowBox = new Material(shadowBoxShader) {hideFlags = HideFlags.HideAndDontSave};
            _shadowRBox = new Material(shadowRBoxShader) {hideFlags = HideFlags.HideAndDontSave};
        }

        static bool OnNotPrepared() {
            initStage = InitStage.Prepared;

            initialFrameCount = Time.frameCount;
            testShader = GetShader(testShaderName);
            var material = new Material(testShader);
            //for Unity 2018 or below, shader is compiled after Shader.Find() call immediately,
            //therefore we can just skip the manually preload if the compilation fails
            if (!material.shader.isSupported || !enableComputeBuffer) {
                ObjectUtils.SafeDestroy(material);
                return OnPrepared(true);
            }

            using (var cmdBuf = new CommandBuffer()) {
                var renderTarget = new RenderTexture(1, 1, 1);
                cmdBuf.SetRenderTarget(renderTarget);

                var mesh = new Mesh {
                    vertices = new[] {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0)},
                    uv = new[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)},
                    triangles = new[] {0, 1, 2}
                };
                cmdBuf.DrawMesh(mesh, Matrix4x4.identity, material);
                cmdBuf.DisableScissorRect();
                Graphics.ExecuteCommandBuffer(cmdBuf);

                ObjectUtils.SafeDestroy(renderTarget);
                ObjectUtils.SafeDestroy(mesh);
            }

            ObjectUtils.SafeDestroy(material);
            
            DoPrepareDefaultShader();
            return true;
        }

        static bool OnPrepared(bool forceReady = false) {
            D.assert(initStage == InitStage.Prepared);
            if (!forceReady && initialFrameCount >= Time.frameCount) {
                initStage = InitStage.Ready;
                return true;
            }

            initStage = InitStage.Ready;
            DoPrepareComputeBufferShader();
            return true;
        }

        static void DoPrepareComputeBufferShader() {
            D.assert(testShader != null);
            var isShaderSupported = testShader.isSupported;
            testShader = null;
            supportComputeBuffer = enableComputeBuffer && SystemInfo.supportsComputeShaders && isShaderSupported;

            if (!supportComputeBuffer) {
                return;
            }

            var convexFillShaderCompute = GetShader("UIWidgets/canvas_convexFill_cb");
            var fill0ShaderCompute = GetShader("UIWidgets/canvas_fill0_cb");
            var fill1ShaderCompute = GetShader("UIWidgets/canvas_fill1_cb");
            var stroke0ShaderCompute = GetShader("UIWidgets/canvas_stroke0_cb");
            var stroke1ShaderCompute = GetShader("UIWidgets/canvas_stroke1_cb");
            var texShaderCompute = GetShader("UIWidgets/canvas_tex_cb");
            var stencilShaderCompute = GetShader("UIWidgets/canvas_stencil_cb");
            var filterShaderCompute = GetShader("UIWidgets/canvas_filter_cb");
            var shadowBoxShaderCompute = GetShader("UIWidgets/ShadowBox_cb");
            var shadowRBoxShaderCompute = GetShader("UIWidgets/ShadowRBox_cb");
            var strokeAlphaShaderCompute = GetShader("UIWidgets/canvas_strokeAlpha_cb");

            _convexFillMat = new MaterialByBlendModeStencilComp(convexFillShaderCompute);
            _fill0Mat = new MaterialByStencilComp(fill0ShaderCompute);
            _fill1Mat = new MaterialByBlendMode(fill1ShaderCompute);
            _stroke0Mat = new MaterialByBlendModeStencilComp(stroke0ShaderCompute);
            _stroke1Mat = new Material(stroke1ShaderCompute) {hideFlags = HideFlags.HideAndDontSave};
            _strokeAlphaMat = new MaterialByBlendModeStencilComp(strokeAlphaShaderCompute);
            _texMat = new MaterialByBlendModeStencilComp(texShaderCompute);
            _stencilMat = new Material(stencilShaderCompute) {hideFlags = HideFlags.HideAndDontSave};
            _filterMat = new Material(filterShaderCompute) {hideFlags = HideFlags.HideAndDontSave};
            _shadowBox = new Material(shadowBoxShaderCompute) {hideFlags = HideFlags.HideAndDontSave};
            _shadowRBox = new Material(shadowRBoxShaderCompute) {hideFlags = HideFlags.HideAndDontSave};
        }

        public static bool isReady() {
            switch (initStage) {
                case InitStage.NotPrepared:
                    return OnNotPrepared();
                case InitStage.Prepared:
                    return OnPrepared();
                case InitStage.Ready:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}