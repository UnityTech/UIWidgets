using Unity.UIWidgets.editor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    static partial class CanvasShader {
        const bool enableDebugLog = false;

        public static bool supportComputeBuffer;

        static void DebugAssert(bool condition, string logMsg) {
            if (enableDebugLog && !condition) {
                Debug.Log(logMsg);
            }
        }

        static bool IsShaderSupported() {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal ||
                   SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan ||
                   SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;
        }

        static void InitShaders() {
            supportComputeBuffer = !WindowConfig.disableComputeBuffer && SystemInfo.supportsComputeShaders && IsShaderSupported();

            if (!supportComputeBuffer) {
                DebugAssert(false, "init default shaders");
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
            else {
                DebugAssert(false, "init computebuffer shaders");
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
        }
    }
}