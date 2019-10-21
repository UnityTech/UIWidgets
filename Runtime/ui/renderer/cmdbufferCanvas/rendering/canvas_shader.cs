using System;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    static class MaterialProps {
        static readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int _dstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int _stencilComp = Shader.PropertyToID("_StencilComp");

        public static void set(Material mat, BlendMode op) {
            if (op == BlendMode.srcOver) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else if (op == BlendMode.srcIn) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.DstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
            }
            else if (op == BlendMode.srcOut) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
            }
            else if (op == BlendMode.srcATop) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.DstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else if (op == BlendMode.dstOver) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.One);
            }
            else if (op == BlendMode.dstIn) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            }
            else if (op == BlendMode.dstOut) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else if (op == BlendMode.dstATop) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            }
            else if (op == BlendMode.plus) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.One);
            }
            else if (op == BlendMode.src) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
            }
            else if (op == BlendMode.dst) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.One);
            }
            else if (op == BlendMode.xor) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else if (op == BlendMode.clear) {
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
            }
            else {
                D.assert(() => {
                    Debug.LogWarning("Not supported BlendMode: " + op + ". Defaults to srcOver");
                    return true;
                });
                mat.SetInt(_srcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                mat.SetInt(_dstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
        }

        public static void set(Material mat, CompareFunction op) {
            mat.SetFloat(_stencilComp, (int) op);
        }
    }

    class MaterialByBlendMode {
        public MaterialByBlendMode(Shader shader) {
            this._shader = shader;
        }

        readonly Shader _shader;
        readonly Material[] _materials = new Material[30];

        public Material getMaterial(BlendMode op) {
            var key = (int) op;
            var mat = this._materials[key];
            if (mat) {
                return mat;
            }

            mat = new Material(this._shader) {hideFlags = HideFlags.HideAndDontSave};
            MaterialProps.set(mat, op);

            this._materials[key] = mat;
            return mat;
        }
    }

    class MaterialByStencilComp {
        public MaterialByStencilComp(Shader shader) {
            this._shader = shader;
        }

        readonly Shader _shader;
        readonly Material[] _materials = new Material[2];

        public Material getMaterial(bool ignoreClip) {
            var key = ignoreClip ? 1 : 0;
            var mat = this._materials[key];
            if (mat) {
                return mat;
            }

            mat = new Material(this._shader) {hideFlags = HideFlags.HideAndDontSave};
            MaterialProps.set(mat, ignoreClip ? CompareFunction.Always : CompareFunction.Equal);

            this._materials[key] = mat;
            return mat;
        }
    }

    class MaterialByBlendModeStencilComp {
        public MaterialByBlendModeStencilComp(Shader shader) {
            this._shader = shader;
        }

        readonly Shader _shader;
        readonly Material[] _materials = new Material[30 * 2];

        public Material getMaterial(BlendMode blend, bool ignoreClip) {
            var key = (int) blend * 2 + (ignoreClip ? 1 : 0);
            var mat = this._materials[key];
            if (mat) {
                return mat;
            }

            mat = new Material(this._shader) {hideFlags = HideFlags.HideAndDontSave};
            MaterialProps.set(mat, blend);
            MaterialProps.set(mat, ignoreClip ? CompareFunction.Always : CompareFunction.Equal);

            this._materials[key] = mat;
            return mat;
        }
    }

    static partial class CanvasShader {
        static MaterialByBlendModeStencilComp _convexFillMat;
        static MaterialByStencilComp _fill0Mat;
        static MaterialByBlendMode _fill1Mat;
        static MaterialByBlendModeStencilComp _stroke0Mat;
        static Material _stroke1Mat;
        static MaterialByBlendModeStencilComp _texMat;
        static Material _stencilMat;
        static Material _filterMat;
        static MaterialByBlendModeStencilComp _strokeAlphaMat;
        static Material _shadowBox;
        static Material _shadowRBox;

        static Shader GetShader(string shaderName) {
            var shader = Shader.Find(shaderName);
            if (shader == null) {
                throw new Exception(shaderName + " not found");
            }

            return shader;
        }

        static CanvasShader() {
            InitShaders();
        }

        static readonly int _viewportId = Shader.PropertyToID("_viewport");
        static readonly int _alphaId = Shader.PropertyToID("_alpha");
        static readonly int _strokeMultId = Shader.PropertyToID("_strokeMult");
        static readonly int _colorId = Shader.PropertyToID("_color");
        static readonly int _shaderMatId = Shader.PropertyToID("_shaderMat");
        static readonly int _shaderTexId = Shader.PropertyToID("_shaderTex");
        static readonly int _leftColorId = Shader.PropertyToID("_leftColor");
        static readonly int _rightColorId = Shader.PropertyToID("_rightColor");
        static readonly int _tileModeId = Shader.PropertyToID("_tileMode");
        static readonly int _biasId = Shader.PropertyToID("_bias");
        static readonly int _scaleId = Shader.PropertyToID("_scale");
        static readonly int _texId = Shader.PropertyToID("_tex");
        static readonly int _texModeId = Shader.PropertyToID("_texMode");
        static readonly int _mfRadiusId = Shader.PropertyToID("_mf_radius");
        static readonly int _mfImgIncId = Shader.PropertyToID("_mf_imgInc");
        static readonly int _mfKernelId = Shader.PropertyToID("_mf_kernel");


        static readonly int _shadowBoxId = Shader.PropertyToID("_sb_box");
        static readonly int _shadowSigmaId = Shader.PropertyToID("_sb_sigma");
        static readonly int _shadowColorId = Shader.PropertyToID("_sb_color");
        static readonly int _shadowCornerId = Shader.PropertyToID("_sb_corner");

        static Vector4 _colorToVector4(uiColor c) {
            return new Vector4(
                c.red / 255f,
                c.green / 255f,
                c.blue / 255f,
                c.alpha / 255f
            );
        }

        static Vector4 _colorToVector4(Color c) {
            return new Vector4(
                c.red / 255f,
                c.green / 255f,
                c.blue / 255f,
                c.alpha / 255f
            );
        }

        static uiMatrix3 _getShaderMatBase(PictureFlusher.State state, uiMatrix3? meshMatrix) {
            if (uiMatrix3.equals(state.matrix, meshMatrix)) {
                return uiMatrix3.I();
            }

            if (meshMatrix == null) {
                return state.invMatrix;
            }

            return uiMatrix3.concat(state.invMatrix, meshMatrix.Value);
        }

        static void _getShaderPassAndProps(
            PictureFlusher.RenderLayer layer, uiPaint paint, uiMatrix3? meshMatrix, float alpha, float strokeMult,
            out int pass, out MaterialPropertyBlockWrapper props) {
            Vector4 viewport = layer.viewport;

            props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);
            props.SetFloat(_alphaId, alpha);
            props.SetFloat(_strokeMultId, strokeMult);

            switch (paint.shader) {
                case null:
                    pass = 0;
                    props.SetVector(_colorId, _colorToVector4(paint.color));
                    return;
                case _LinearGradient linear:
                    pass = 1;
                    props.SetMatrix(_shaderMatId, linear.getGradientMat(
                        _getShaderMatBase(layer.currentState, meshMatrix)).toMatrix4x4());
                    props.SetTexture(_shaderTexId, linear.gradientTex.texture);
                    props.SetVector(_leftColorId, _colorToVector4(linear.leftColor));
                    props.SetVector(_rightColorId, _colorToVector4(linear.rightColor));
                    props.SetInt(_tileModeId, (int) linear.tileMode);
                    return;
                case _RadialGradient radial:
                    pass = 2;
                    props.SetMatrix(_shaderMatId, radial.getGradientMat(
                        _getShaderMatBase(layer.currentState, meshMatrix)).toMatrix4x4());
                    props.SetTexture(_shaderTexId, radial.gradientTex.texture);
                    props.SetVector(_leftColorId, _colorToVector4(radial.leftColor));
                    props.SetVector(_rightColorId, _colorToVector4(radial.rightColor));
                    props.SetInt(_tileModeId, (int) radial.tileMode);
                    return;
                case _SweepGradient sweep:
                    pass = 3;
                    props.SetMatrix(_shaderMatId, sweep.getGradientMat(
                        _getShaderMatBase(layer.currentState, meshMatrix)).toMatrix4x4());
                    props.SetTexture(_shaderTexId, sweep.gradientTex.texture);
                    props.SetVector(_leftColorId, _colorToVector4(sweep.leftColor));
                    props.SetVector(_rightColorId, _colorToVector4(sweep.rightColor));
                    props.SetInt(_tileModeId, (int) sweep.tileMode);
                    props.SetFloat(_biasId, sweep.bias);
                    props.SetFloat(_scaleId, sweep.scale);
                    return;
                case ImageShader image:
                    pass = 4;
                    props.SetMatrix(_shaderMatId, image.getShaderMat(
                        _getShaderMatBase(layer.currentState, meshMatrix)).toMatrix4x4());
                    props.SetTexture(_shaderTexId, image.image.texture);
                    props.SetInt(_tileModeId, (int) image.tileMode);
                    return;
                default:
                    throw new Exception("Unknown paint.shader: " + paint.shader);
            }
        }

        public static PictureFlusher.CmdDraw convexFill(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh) {
            var mat = _convexFillMat.getMaterial(paint.blendMode, layer.ignoreClip);
            
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, 0.0f, out var pass, out var props);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw fill0(PictureFlusher.RenderLayer layer, uiMeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _fill0Mat.getMaterial(layer.ignoreClip);

            var pass = 0;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw fill1(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh) {
            var mat = _fill1Mat.getMaterial(paint.blendMode);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, 0.0f, out var pass, out var props);

            var ret = PictureFlusher.CmdDraw.create(
                mesh: mesh.boundsMesh,
                pass: pass,
                material: mat,
                properties: props
            );

            ObjectPool<uiMeshMesh>.release(mesh);
            return ret;
        }

        public static PictureFlusher.CmdDraw stroke0(PictureFlusher.RenderLayer layer, uiPaint paint,
            float alpha, uiMeshMesh mesh) {
            var mat = _stroke0Mat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, alpha, 0.0f, out var pass, out var props);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw stroke1(PictureFlusher.RenderLayer layer, uiMeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stroke1Mat;

            var pass = 0;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw strokeAlpha(PictureFlusher.RenderLayer layer, uiPaint paint, float alpha, float strokeMult, uiMeshMesh mesh) {
            var mat = _strokeAlphaMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, alpha, strokeMult, out var pass, out var props);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw stencilClear(
            PictureFlusher.RenderLayer layer, uiMeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 0;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw stencil0(PictureFlusher.RenderLayer layer, uiMeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 1;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw stencil1(PictureFlusher.RenderLayer layer, uiMeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 2;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw tex(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh, Image image) {
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, 0.0f, out var pass, out var props);

            image.texture.filterMode = paint.filterMode;
            props.SetTexture(_texId, image.texture);
            props.SetInt(_texModeId, image.texture is RenderTexture ? 1 : 0); // pre alpha if RT else post alpha

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props,
                image: image // keep a reference to avoid GC.
            );
        }

        public static PictureFlusher.CmdDraw texRT(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh, PictureFlusher.RenderLayer renderLayer) {
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, 0.0f, out var pass, out var props);
            props.SetInt(_texModeId, 1); // pre alpha

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props,
                layerId: renderLayer.rtID
            );
        }

        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh, Texture tex) {
            return texAlpha(layer, paint, mesh, null, tex);
        }

        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, uiPaint paint,
            TextBlobMesh textMesh, Texture tex) {
            return texAlpha(layer, paint, null, textMesh, tex);
        }

        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, uiPaint paint,
            uiMeshMesh mesh, TextBlobMesh textMesh, Texture tex) {
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            var meshMatrix = mesh != null ? mesh.matrix : textMesh.matrix;
            _getShaderPassAndProps(layer, paint, meshMatrix, 1.0f, 0.0f, out var pass, out var props);
            tex.filterMode = paint.filterMode;
            props.SetTexture(_texId, tex);
            props.SetInt(_texModeId, 2); // alpha only

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                textMesh: textMesh,
                pass: pass,
                material: mat,
                properties: props
            );
        }

        public static PictureFlusher.CmdDraw maskFilter(PictureFlusher.RenderLayer layer, uiMeshMesh mesh,
            PictureFlusher.RenderLayer renderLayer, float radius, Vector2 imgInc, float[] kernel) {
            Vector4 viewport = layer.viewport;
            var mat = _filterMat;

            var pass = 0;
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);

            props.SetFloat(_mfRadiusId, radius);
            props.SetVector(_mfImgIncId, imgInc);
            props.SetFloatArray(_mfKernelId, kernel);

            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: pass,
                material: mat,
                properties: props,
                layerId: renderLayer.rtID
            );
        }

        public static PictureFlusher.CmdDraw fastShadow(PictureFlusher.RenderLayer layer, uiMeshMesh mesh, float sigma,
            bool isRect, bool isCircle, float corner, Vector4 bound, uiColor color) {
            Vector4 viewport = layer.viewport;
            var mat = _shadowBox;
            if (!isRect) {
                mat = _shadowRBox;
            }
            
            var props = ObjectPool<MaterialPropertyBlockWrapper>.alloc();
            props.SetVector(_viewportId, viewport);
            props.SetFloat(_shadowSigmaId, sigma);
            props.SetVector(_shadowBoxId, bound);
            props.SetVector(_shadowColorId, _colorToVector4(color));
            if (!isRect) {
                props.SetFloat(_shadowCornerId, corner);
            }
            
            return PictureFlusher.CmdDraw.create(
                mesh: mesh,
                pass: 0,
                material: mat,
                properties: props
            );
        }
    }
}