using System;
using Unity.UIWidgets.painting;
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
                Debug.LogWarning("Not supported BlendMode: " + op + ". Defaults to srcOver");
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

    static class CanvasShader {
        static readonly MaterialByBlendModeStencilComp _convexFillMat;
        static readonly MaterialByStencilComp _fill0Mat;
        static readonly MaterialByBlendMode _fill1Mat;
        static readonly MaterialByBlendModeStencilComp _stroke0Mat;
        static readonly Material _stroke1Mat;
        static readonly MaterialByBlendModeStencilComp _texMat;
        static readonly Material _stencilMat;
        static readonly Material _filterMat;

        static CanvasShader() {
            var convexFillShader = Shader.Find("UIWidgets/canvas_convexFill");
            if (convexFillShader == null) {
                throw new Exception("UIWidgets/canvas_convexFill not found");
            }

            var fill0Shader = Shader.Find("UIWidgets/canvas_fill0");
            if (fill0Shader == null) {
                throw new Exception("UIWidgets/canvas_fill0 not found");
            }

            var fill1Shader = Shader.Find("UIWidgets/canvas_fill1");
            if (fill1Shader == null) {
                throw new Exception("UIWidgets/canvas_fill1 not found");
            }

            var stroke0Shader = Shader.Find("UIWidgets/canvas_stroke0");
            if (stroke0Shader == null) {
                throw new Exception("UIWidgets/canvas_stroke0 not found");
            }

            var stroke1Shader = Shader.Find("UIWidgets/canvas_stroke1");
            if (stroke1Shader == null) {
                throw new Exception("UIWidgets/canvas_stroke1 not found");
            }

            var texShader = Shader.Find("UIWidgets/canvas_tex");
            if (texShader == null) {
                throw new Exception("UIWidgets/canvas_tex not found");
            }

            var stencilShader = Shader.Find("UIWidgets/canvas_stencil");
            if (stencilShader == null) {
                throw new Exception("UIWidgets/canvas_stencil not found");
            }

            var filterShader = Shader.Find("UIWidgets/canvas_filter");
            if (filterShader == null) {
                throw new Exception("UIWidgets/canvas_filter not found");
            }

            _convexFillMat = new MaterialByBlendModeStencilComp(convexFillShader);
            _fill0Mat = new MaterialByStencilComp(fill0Shader);
            _fill1Mat = new MaterialByBlendMode(fill1Shader);
            _stroke0Mat = new MaterialByBlendModeStencilComp(stroke0Shader);
            _stroke1Mat = new Material(stroke1Shader) {hideFlags = HideFlags.HideAndDontSave};
            _texMat = new MaterialByBlendModeStencilComp(texShader);
            _stencilMat = new Material(stencilShader) {hideFlags = HideFlags.HideAndDontSave};
            _filterMat = new Material(filterShader) {hideFlags = HideFlags.HideAndDontSave};
        }
        
        static readonly int _viewportId = Shader.PropertyToID("_viewport");
        static readonly int _alphaId = Shader.PropertyToID("_alpha");
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

        static Vector4 _colorToVector4(Color c) {
            return new Vector4(
                c.red / 255f,
                c.green / 255f,
                c.blue / 255f,
                c.alpha / 255f
            );
        }

        static Matrix3 _getShaderMatBase(PictureFlusher.State state, Matrix3 meshMatrix) {
            if (state.matrix == meshMatrix) {
                return Matrix3.I();
            }

            if (meshMatrix == null) {
                return state.invMatrix;
            }

            return Matrix3.concat(state.invMatrix, meshMatrix);
        }

        static void _getShaderPassAndProps(
            PictureFlusher.RenderLayer layer, Paint paint, Matrix3 meshMatrix, float alpha,
            out int pass, out MaterialPropertyBlock props) {
            Vector4 viewport = layer.viewport;

            props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);
            props.SetFloat(_alphaId, alpha);

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

        public static PictureFlusher.CmdDraw convexFill(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh) {
            var mat = _convexFillMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, out var pass, out var props);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw fill0(PictureFlusher.RenderLayer layer, MeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _fill0Mat.getMaterial(layer.ignoreClip);

            var pass = 0;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw fill1(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh) {
            var mat = _fill1Mat.getMaterial(paint.blendMode);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, out var pass, out var props);

            return new PictureFlusher.CmdDraw {
                mesh = mesh.boundsMesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw stroke0(PictureFlusher.RenderLayer layer, Paint paint,
            float alpha, MeshMesh mesh) {
            var mat = _stroke0Mat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, alpha, out var pass, out var props);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw stroke1(PictureFlusher.RenderLayer layer, MeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stroke1Mat;

            var pass = 0;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw stencilClear(
            PictureFlusher.RenderLayer layer, MeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 0;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw stencil0(PictureFlusher.RenderLayer layer, MeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 1;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw stencil1(PictureFlusher.RenderLayer layer, MeshMesh mesh) {
            Vector4 viewport = layer.viewport;
            var mat = _stencilMat;

            var pass = 2;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw tex(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh, Image image) {
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, out var pass, out var props);

            image.texture.filterMode = paint.filterMode;
            props.SetTexture(_texId, image.texture);
            props.SetInt(_texModeId, image.texture is RenderTexture ? 1 : 0); // pre alpha if RT else post alpha

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
                image = image, // keep a reference to avoid GC.
            };
        }

        public static PictureFlusher.CmdDraw texRT(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh, PictureFlusher.RenderLayer renderLayer) {
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            _getShaderPassAndProps(layer, paint, mesh.matrix, 1.0f, out var pass, out var props);
            props.SetInt(_texModeId, 1); // pre alpha

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
                layer = renderLayer,
            };
        }

        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh, Texture tex) {
            return texAlpha(layer, paint, mesh, null, tex);
        }
        
        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, Paint paint,
            TextBlobMesh textMesh, Texture tex) {
            return texAlpha(layer, paint, null, textMesh, tex);
        }
        
        public static PictureFlusher.CmdDraw texAlpha(PictureFlusher.RenderLayer layer, Paint paint,
            MeshMesh mesh, TextBlobMesh textMesh, Texture tex) {
            
            var mat = _texMat.getMaterial(paint.blendMode, layer.ignoreClip);
            var meshMatrix = mesh != null ? mesh.matrix : textMesh.matrix;
            _getShaderPassAndProps(layer, paint, meshMatrix, 1.0f, out var pass, out var props);
            tex.filterMode = paint.filterMode;
            props.SetTexture(_texId, tex);
            props.SetInt(_texModeId, 2); // alpha only

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                textMesh = textMesh,
                pass = pass,
                material = mat,
                properties = props,
            };
        }

        public static PictureFlusher.CmdDraw maskFilter(PictureFlusher.RenderLayer layer, MeshMesh mesh,
            PictureFlusher.RenderLayer renderLayer, float radius, Vector2 imgInc, float[] kernel) {
            Vector4 viewport = layer.viewport;
            var mat = _filterMat;

            var pass = 0;
            var props = new MaterialPropertyBlock();
            props.SetVector(_viewportId, viewport);

            props.SetFloat(_mfRadiusId, radius);
            props.SetVector(_mfImgIncId, imgInc);
            props.SetFloatArray(_mfKernelId, kernel);

            return new PictureFlusher.CmdDraw {
                mesh = mesh,
                pass = pass,
                material = mat,
                properties = props,
                layer = renderLayer,
            };
        }
    }
}