using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    public class CommandBufferCanvas : Canvas {
        readonly RenderTexture _renderTexture;
        readonly float _fringeWidth;
        readonly float _devicePixelRatio;
        readonly MeshPool _meshPool;
        readonly List<RenderLayer> _layers = new List<RenderLayer>();
        int _saveCount;

        static readonly List<int> _imageTriangles = new List<int>(12) {
            0, 1, 2, 0, 2, 1,
            0, 2, 3, 0, 3, 2,
        };

        static readonly List<int> _imageNineTriangles = new List<int>(12 * 9) {
            0, 4, 1, 1, 4, 5,
            0, 1, 4, 1, 5, 4,
            1, 5, 2, 2, 5, 6,
            1, 2, 5, 2, 6, 5,
            2, 6, 3, 3, 6, 7,
            2, 3, 6, 3, 7, 6,
            4, 8, 5, 5, 8, 9,
            4, 5, 8, 5, 9, 8,
            5, 9, 6, 6, 9, 10,
            5, 6, 9, 6, 10, 9,
            6, 10, 7, 7, 10, 11,
            6, 7, 10, 7, 11, 10,
            8, 12, 9, 9, 12, 13,
            8, 9, 12, 9, 13, 12,
            9, 13, 10, 10, 13, 14,
            9, 10, 13, 10, 14, 13,
            10, 14, 11, 11, 14, 15,
            10, 11, 14, 11, 15, 14,
        };

        public CommandBufferCanvas(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool) {
            D.assert(renderTexture);

            this._renderTexture = renderTexture;
            this._fringeWidth = 1.0f / devicePixelRatio;
            this._devicePixelRatio = devicePixelRatio;
            this._meshPool = meshPool;

            this.reset();
        }

        RenderLayer _getLayer() {
            D.assert(this._layers.Count > 0);
            return this._layers.Last();
        }

        State _getState() {
            var layer = this._getLayer();
            D.assert(layer.states.Count > 0);
            return layer.states.Last();
        }

        public int getSaveCount() {
            return this._saveCount;
        }

        public void save() {
            this._saveCount++;

            var layer = this._getLayer();
            layer.states.Add(this._getState().copy());
            layer.clipStack.save();
        }

        public void saveLayer(Rect bounds, Paint paint) {
            D.assert(bounds != null);
            D.assert(bounds.width > 0);
            D.assert(bounds.height > 0);
            D.assert(paint != null);

            this._saveCount++;

            var state = this._getState();
            var textureWidth = Mathf.CeilToInt(
                (float) bounds.width *
                XformUtils.getScaleX(state.xform) *
                this._devicePixelRatio);
            textureWidth = Mathf.Max(textureWidth, 1);

            var textureHeight = Mathf.CeilToInt(
                (float) bounds.height *
                XformUtils.getScaleY(state.xform) *
                this._devicePixelRatio);
            textureHeight = Mathf.Max(textureHeight, 1);

            var parentLayer = this._getLayer();
            var layer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = bounds,
                layerPaint = new Paint {
                    color = paint.color,
                    blendMode = paint.blendMode,
                }
            };

            parentLayer.layers.Add(layer);
            this._layers.Add(layer);
        }

        public void restore() {
            this._saveCount--;

            var layer = this._getLayer();
            D.assert(layer.states.Count > 0);
            if (layer.states.Count > 1) {
                layer.states.RemoveAt(layer.states.Count - 1);
                layer.clipStack.restore();
                return;
            }

            this._layers.RemoveAt(this._layers.Count - 1);
            var state = this._getState();

            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            const float uvx0 = 0.0f;
            const float uvx1 = 1.0f;
            const float uvy0 = 1.0f;
            const float uvy1 = 0.0f;

            float x, y;
            var bounds = layer.layerBounds;
            PathUtils.transformPoint(out x, out y, state.xform,
                (float) bounds.left, (float) bounds.top);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx0, uvy0));
            PathUtils.transformPoint(out x, out y, state.xform,
                (float) bounds.left, (float) bounds.bottom);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx0, uvy1));
            PathUtils.transformPoint(out x, out y, state.xform,
                (float) bounds.right, (float) bounds.bottom);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx1, uvy1));
            PathUtils.transformPoint(out x, out y, state.xform,
                (float) bounds.right, (float) bounds.top);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx1, uvy0));

            var mesh = new MeshMesh(vertices, _imageTriangles, uv);

            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var mat = this._getMat(layer.layerPaint);
            var properties = this._getMatPropsForImage(null, layer.layerPaint);

            this._getLayer().draws.Add(
                new RenderDraw {
                    mesh = mesh,
                    pass = CanvasShaderPass.texrtPass0,
                    material = mat,
                    properties = properties,
                    layer = layer
                }
            );
        }

        public void translate(double dx, double dy) {
            var state = this._getState();
            float[] t = new float[6];
            XformUtils.transformTranslate(t, (float) dx, (float) dy);
            XformUtils.transformPremultiply(state.xform, t);
        }

        public void scale(double sx, double? sy = null) {
            var state = this._getState();
            float[] t = new float[6];
            XformUtils.transformScale(t, (float) sx, (float) (sy ?? sx));
            XformUtils.transformPremultiply(state.xform, t);
        }

        public void rotate(double radians, Offset offset = null) {
            var state = this._getState();
            float[] t = new float[6];

            if (offset != null) {
                XformUtils.transformTranslate(t, (float) -offset.dx, (float) -offset.dy);
                XformUtils.transformPremultiply(state.xform, t);
            }

            XformUtils.transformRotate(t, (float) radians);
            XformUtils.transformPremultiply(state.xform, t);

            if (offset != null) {
                XformUtils.transformTranslate(t, (float) offset.dx, (float) offset.dy);
                XformUtils.transformPremultiply(state.xform, t);
            }
        }

        public void skew(double sx, double sy) {
            var state = this._getState();
            float[] t = new float[6];
            XformUtils.transformSkew(t, (float) sx, (float) sy);
            XformUtils.transformPremultiply(state.xform, t);
        }

        public void concat(Matrix3 matrix) {
            var state = this._getState();
            float[] t = XformUtils.fromMatrix3(matrix);
            XformUtils.transformPremultiply(state.xform, t);
        }

        public Matrix3 getTotalMatrix() {
            var state = this._getState();
            return XformUtils.toMatrix3(state.xform);
        }

        public void resetMatrix() {
            var state = this._getState();
            XformUtils.transformIdentity(state.xform);
        }

        public void setMatrix(Matrix3 matrix) {
            var state = this._getState();
            state.xform = XformUtils.fromMatrix3(matrix);
        }

        public float getDevicePixelRatio() {
            return this._devicePixelRatio;
        }

        public void clipRect(Rect rect) {
            var path = new Path();
            path.addRect(rect);
            this.clipPath(path);
        }

        public void clipRRect(RRect rrect) {
            var path = new Path();
            path.addRRect(rrect);
            this.clipPath(path);
        }

        public void clipPath(Path path) {
            var layer = this._getLayer();
            var state = this._getState();
            layer.clipStack.clipPath(path, state.xform, this._devicePixelRatio);
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            var path = new Path();
            path.moveTo(from.dx, from.dy);
            path.lineTo(to.dx, to.dy);
            this.drawPath(path, paint);
        }

        public void drawRect(Rect rect, Paint paint) {
            var path = new Path();
            path.addRect(rect);
            this.drawPath(path, paint);
        }

        public void drawRRect(RRect rrect, Paint paint) {
            var path = new Path();
            path.addRRect(rrect);
            this.drawPath(path, paint);
        }

        public void drawDRRect(RRect outer, RRect inner, Paint paint) {
            var path = new Path();
            path.addRRect(outer);
            path.addRRect(inner);
            path.winding(PathWinding.clockwise);
            this.drawPath(path, paint);
        }

        public void drawOval(Rect rect, Paint paint) {
            var w = rect.width / 2;
            var h = rect.height / 2;
            var path = new Path();
            path.addEllipse(rect.left + w, rect.top + h, w, h);
            this.drawPath(path, paint);
        }

        public void drawCircle(Offset c, double radius, Paint paint) {
            var path = new Path();
            path.addCircle(c.dx, c.dy, radius);
            this.drawPath(path, paint);
        }

        public void drawArc(Rect rect, double startAngle, double sweepAngle, bool useCenter, Paint paint) {
            //var path = new Path();
        }

        static Vector4 _premulColor(Color c) {
            return new Vector4(
                c.red * c.alpha / (255f * 255f),
                c.green * c.alpha / (255f * 255f),
                c.blue * c.alpha / (255f * 255f),
                c.alpha / 255f
            );
        }

        static Vector4 _color(Color c) {
            return new Vector4(
                c.red / 255f,
                c.green / 255f,
                c.blue / 255f,
                c.alpha / 255f
            );
        }

        MaterialPropertyBlock _getMatProps(Paint paint, float alpha = 1.0f) {
            var properties = new MaterialPropertyBlock();
            properties.SetVector("_viewport", this._getViewport());

            if (paint.shader is Gradient gradient) {
                var innerCol = gradient.innerColor;
                var outerCol = gradient.outerColor;

                if (alpha != 1.0f) {
                    innerCol = innerCol.withOpacity(innerCol.opacity * alpha);
                    outerCol = outerCol.withOpacity(outerCol.opacity * alpha);
                }

                var matrix = Matrix4x4.zero;
                matrix[0, 0] = gradient.invXform[0];
                matrix[1, 0] = gradient.invXform[1];
                matrix[0, 1] = gradient.invXform[2];
                matrix[1, 1] = gradient.invXform[3];
                matrix[0, 2] = gradient.invXform[4];
                matrix[1, 2] = gradient.invXform[5];

                properties.SetMatrix("_paintMat", matrix);
                properties.SetVector("_innerCol", _premulColor(innerCol));
                properties.SetVector("_outerCol", _premulColor(outerCol));
                properties.SetVector("_extent", new Vector2(gradient.extent[0], gradient.extent[1]));
                properties.SetFloat("_radius", gradient.radius);
                properties.SetFloat("_feather", gradient.feather);
            }
            else {
                var innerCol = paint.color;
                var outerCol = paint.color;

                if (alpha != 1.0f) {
                    innerCol = innerCol.withOpacity(innerCol.opacity * alpha);
                    outerCol = outerCol.withOpacity(outerCol.opacity * alpha);
                }

                properties.SetMatrix("_paintMat", Matrix4x4.identity);
                properties.SetVector("_innerCol", _premulColor(innerCol));
                properties.SetVector("_outerCol", _premulColor(outerCol));
                properties.SetVector("_extent", new Vector2(0.0f, 0.0f));
                properties.SetFloat("_radius", 0.0f);
                properties.SetFloat("_feather", 1.0f);
            }

            return properties;
        }

        bool _applyClip(Rect queryBounds) {
            var layer = this._getLayer();
            var layerBounds = layer.layerBounds;
            ReducedClip reducedClip = new ReducedClip(layer.clipStack, layerBounds, queryBounds);
            if (reducedClip.isEmpty()) {
                return false;
            }

            var scissor = reducedClip.scissor;
            var deviceScissor = Rect.fromLTRB(
                scissor.left - layerBounds.left, layerBounds.bottom - scissor.bottom,
                scissor.right - layerBounds.left, layerBounds.bottom - scissor.top
            ).scale(layer.width / layerBounds.width, layer.height / layerBounds.height);

            layer.draws.Add(new RenderScissor {
                deviceScissor = deviceScissor.roundOut(),
            });

            if (this._mustRenderClip(reducedClip.maskGenID(), reducedClip.scissor)) {
                var mat = this._getMat(null);
                var properties = new MaterialPropertyBlock();
                properties.SetVector("_viewport", this._getViewport());

                var boundsMesh = new MeshMesh(reducedClip.scissor);
                this._getLayer().draws.Add(new RenderDraw {
                    mesh = boundsMesh,
                    pass = CanvasShaderPass.stencilClear,
                    material = mat,
                    properties = properties,
                });

                for (var i = 0; i < reducedClip.maskElements.Count; i++) {
                    var maskElement = reducedClip.maskElements[i];

                    this._getLayer().draws.Add(new RenderDraw {
                        mesh = maskElement.mesh,
                        pass = CanvasShaderPass.stencilIntersect0,
                        material = mat,
                        properties = properties,
                    });
                    this._getLayer().draws.Add(new RenderDraw {
                        mesh = boundsMesh,
                        pass = CanvasShaderPass.stencilIntersect1,
                        material = mat,
                        properties = properties,
                    });
                }

                this._setLastClipGenId(reducedClip.maskGenID(), reducedClip.scissor);
            }

            return true;
        }

        RenderLayer _createMaskLayer(Path path, Paint paint, RenderLayer parentLayer, Rect maskBounds) {
            var textureWidth = Mathf.CeilToInt(
                (float) maskBounds.width * this._devicePixelRatio);
            textureWidth = Mathf.Max(1, textureWidth);

            var textureHeight = Mathf.CeilToInt((float) maskBounds.height * this._devicePixelRatio);
            textureHeight = Mathf.Max(1, textureHeight);

            var maskLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskBounds,
            };

            parentLayer.layers.Add(maskLayer);
            this._layers.Add(maskLayer);

            var parentState = parentLayer.states.Last();
            var maskState = maskLayer.states.Last();
            XformUtils.transformCopy(maskState.xform, parentState.xform);

            var maskPaint = Paint.shapeOnly(paint);
            this._drawPath(path, maskPaint);

            var removed = this._layers.removeLast();
            D.assert(removed == maskLayer);

            return maskLayer;
        }

        RenderLayer _createBlurLayer(RenderLayer maskLayer, float sigma, RenderLayer parentLayer) {
            sigma = BlurUtils.adjustSigma(sigma, out var scaleFactor, out var radius);

            var textureWidth = Mathf.CeilToInt((float) maskLayer.width / scaleFactor);
            textureWidth = Mathf.Max(1, textureWidth);

            var textureHeight = Mathf.CeilToInt((float) maskLayer.height / scaleFactor);
            textureHeight = Mathf.Max(1, textureHeight);

            var blurXLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskLayer.layerBounds,
            };

            parentLayer.layers.Add(blurXLayer);

            var blurYLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskLayer.layerBounds,
            };

            parentLayer.layers.Add(blurYLayer);

            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            const float uvx0 = 0.0f;
            const float uvx1 = 1.0f;
            const float uvy0 = 1.0f;
            const float uvy1 = 0.0f;

            vertices.Add(new Vector2((float) maskLayer.layerBounds.left, (float) maskLayer.layerBounds.top));
            uv.Add(new Vector2(uvx0, uvy0));
            vertices.Add(new Vector2((float) maskLayer.layerBounds.left, (float) maskLayer.layerBounds.bottom));
            uv.Add(new Vector2(uvx0, uvy1));
            vertices.Add(new Vector2((float) maskLayer.layerBounds.right, (float) maskLayer.layerBounds.bottom));
            uv.Add(new Vector2(uvx1, uvy1));
            vertices.Add(new Vector2((float) maskLayer.layerBounds.right, (float) maskLayer.layerBounds.top));
            uv.Add(new Vector2(uvx1, uvy0));

            var blurMesh = new MeshMesh(vertices, _imageTriangles, uv);

            var emptyPaint = new Paint();

            var kernel = BlurUtils.get1DGaussianKernel(sigma, radius);

            this._layers.Add(blurXLayer);

            {
                var mat = this._getMat(emptyPaint);
                var properties = this._getMatPropsForImage(null, emptyPaint);

                properties.SetFloat("_mf_radius", radius);
                properties.SetVector("_mf_imgInc", new Vector2(1f / textureWidth, 0));
                properties.SetFloatArray("_mf_kernel", kernel);

                blurXLayer.draws.Add(
                    new RenderDraw {
                        mesh = blurMesh,
                        pass = CanvasShaderPass.mfPass0,
                        material = mat,
                        properties = properties,
                        layer = maskLayer
                    }
                );
            }


            var removed = this._layers.removeLast();
            D.assert(removed == blurXLayer);

            this._layers.Add(blurYLayer);

            {
                var mat = this._getMat(emptyPaint);
                var properties = this._getMatPropsForImage(null, emptyPaint);

                properties.SetFloat("_mf_radius", radius);
                properties.SetVector("_mf_imgInc", new Vector2(0, -1f / textureHeight));
                properties.SetFloatArray("_mf_kernel", kernel);

                blurYLayer.draws.Add(
                    new RenderDraw {
                        mesh = blurMesh,
                        pass = CanvasShaderPass.mfPass0,
                        material = mat,
                        properties = properties,
                        layer = blurXLayer
                    }
                );
            }

            removed = this._layers.removeLast();
            D.assert(removed == blurYLayer);

            return blurYLayer;
        }

        void _drawPathWithMaskFilter(Path path, Paint paint, MaskFilter maskFilter) {
            var layer = this._getLayer();
            var clipBounds = layer.layerBounds;

            Rect stackBounds;
            bool iior;
            layer.clipStack.getBounds(out stackBounds, out iior);

            if (stackBounds != null) {
                clipBounds = clipBounds.intersect(stackBounds);
            }

            if (clipBounds.isEmpty) {
                return;
            }

            var state = this._getState();
            var cache = path.flatten(state.xform, this._devicePixelRatio);

            bool convex;
            var mesh = cache.getFillMesh(out convex);
            var meshBounds = mesh.bounds;

            float sigma = XformUtils.mapRadius(state.xform, (float) maskFilter.sigma);
            if (sigma <= 0) {
                return;
            }

            float sigma3 = 3 * sigma;
            var maskBounds = meshBounds.inflate(sigma3);
            maskBounds = maskBounds.intersect(clipBounds.inflate(sigma3));
            if (maskBounds.isEmpty) {
                return;
            }

            var maskLayer = this._createMaskLayer(path, paint, layer, maskBounds);

            var blurLayer = this._createBlurLayer(maskLayer, sigma, layer);

            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            const float uvx0 = 0.0f;
            const float uvx1 = 1.0f;
            const float uvy0 = 1.0f;
            const float uvy1 = 0.0f;

            vertices.Add(new Vector2((float) maskBounds.left, (float) maskBounds.top));
            uv.Add(new Vector2(uvx0, uvy0));
            vertices.Add(new Vector2((float) maskBounds.left, (float) maskBounds.bottom));
            uv.Add(new Vector2(uvx0, uvy1));
            vertices.Add(new Vector2((float) maskBounds.right, (float) maskBounds.bottom));
            uv.Add(new Vector2(uvx1, uvy1));
            vertices.Add(new Vector2((float) maskBounds.right, (float) maskBounds.top));
            uv.Add(new Vector2(uvx1, uvy0));

            var blurMesh = new MeshMesh(vertices, _imageTriangles, uv);

            if (!this._applyClip(blurMesh.bounds)) {
                return;
            }

            var mat = this._getMat(paint);
            var properties = this._getMatPropsForImage(null, paint);

            layer.draws.Add(
                new RenderDraw {
                    mesh = blurMesh,
                    pass = CanvasShaderPass.texrtPass0,
                    material = mat,
                    properties = properties,
                    layer = blurLayer
                }
            );
        }

        void _drawPath(Path path, Paint paint) {
            D.assert(path != null);
            D.assert(paint != null);

            if (paint.style == PaintingStyle.fill) {
                var state = this._getState();
                var cache = path.flatten(state.xform, this._devicePixelRatio);

                bool convex;
                var mesh = cache.getFillMesh(out convex);

                if (!this._applyClip(mesh.bounds)) {
                    return;
                }

                var mat = this._getMat(paint);
                var properties = this._getMatProps(paint);
                if (convex) {
                    this._getLayer().draws.Add(new RenderDraw {
                        mesh = mesh,
                        pass = CanvasShaderPass.convexFill,
                        material = mat,
                        properties = properties,
                    });
                }
                else {
                    this._getLayer().draws.Add(new RenderDraw {
                        mesh = mesh,
                        pass = CanvasShaderPass.fillPass0,
                        material = mat,
                        properties = properties,
                    });
                    this._getLayer().draws.Add(new RenderDraw {
                        mesh = mesh.boundsMesh,
                        pass = CanvasShaderPass.fillPass1,
                        material = mat,
                        properties = properties,
                    });
                }
            }
            else {
                var state = this._getState();
                float scale = XformUtils.getAverageScale(state.xform);
                float strokeWidth = ((float) paint.strokeWidth * scale).clamp(0, 200.0f);
                float alpha = 1.0f;

                if (strokeWidth == 0) {
                    strokeWidth = this._fringeWidth;
                }
                else if (strokeWidth < this._fringeWidth) {
                    // If the stroke width is less than pixel size, use alpha to emulate coverage.
                    // Since coverage is area, scale by alpha*alpha.
                    alpha = (strokeWidth / this._fringeWidth).clamp(0.0f, 1.0f);
                    alpha *= alpha;
                    strokeWidth = this._fringeWidth;
                }

                var cache = path.flatten(state.xform, this._devicePixelRatio);
                var mesh = cache.getStrokeMesh(
                    strokeWidth * 0.5f,
                    paint.strokeCap,
                    paint.strokeJoin,
                    (float) paint.strokeMiterLimit);

                if (!this._applyClip(mesh.bounds)) {
                    return;
                }

                var mat = this._getMat(paint);
                var properties = this._getMatProps(paint, alpha);
                this._getLayer().draws.Add(new RenderDraw {
                    mesh = mesh,
                    pass = CanvasShaderPass.strokePass0,
                    material = mat,
                    properties = properties,
                });
                this._getLayer().draws.Add(new RenderDraw {
                    mesh = mesh,
                    pass = CanvasShaderPass.strokePass1,
                    material = mat,
                    properties = properties,
                });
            }
        }

        public void drawPath(Path path, Paint paint) {
            D.assert(path != null);
            D.assert(paint != null);

            if (paint.maskFilter != null) {
                this._drawPathWithMaskFilter(path, paint, paint.maskFilter);
                return;
            }

            this._drawPath(path, paint);
        }

        public void drawImage(Image image, Offset offset, Paint paint) {
            D.assert(image != null);
            D.assert(offset != null);
            D.assert(paint != null);

            this.drawImageRect(image,
                null,
                Rect.fromLTWH(offset.dx, offset.dy, image.width / this._devicePixelRatio,
                    image.height / this._devicePixelRatio),
                paint);
        }

        Vector4 _getViewport() {
            var layer = this._getLayer();
            return new Vector4(
                (float) layer.layerBounds.left,
                (float) layer.layerBounds.top,
                (float) layer.layerBounds.width,
                (float) layer.layerBounds.height);
        }

        static Dictionary<BlendMode, Material> _materials = new Dictionary<BlendMode, Material>();
        static readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int _dstBlend = Shader.PropertyToID("_DstBlend");

        Material _getMat(Paint paint) {
            var op = paint == null ? BlendMode.srcOver : paint.blendMode;

            Material mat;
            if (_materials.TryGetValue(op, out mat) && mat) {
                return mat;
            }

            var canvasShader = Shader.Find("UIWidgets/canvas");
            if (canvasShader == null) {
                throw new Exception("UIWidgets/canvas not found");
            }

            mat = new Material(canvasShader);
            mat.hideFlags = HideFlags.HideAndDontSave;
            _materials[op] = mat;

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

            return mat;
        }

        MaterialPropertyBlock _getMatPropsForImage(Texture image, Paint paint) {
            var properties = new MaterialPropertyBlock();
            properties.SetVector("_viewport", this._getViewport());
            properties.SetVector("_innerCol", _color(paint.color));
            if (image != null) {
                image.filterMode = paint.filterMode;
                properties.SetTexture("_tex", image);
            }

            return properties;
        }

        public void drawImageRect(Image image, Rect dst, Paint paint) {
            this.drawImageRect(image, null, dst, paint);
        }

        public void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(dst != null);
            D.assert(paint != null);

            if (src == null) {
                src = Rect.fromLTWH(0, 0, image.width, image.height);
            }

            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            float uvx0 = (float) src.left / image.width;
            float uvx1 = (float) src.right / image.width;
            float uvy0 = 1.0f - (float) src.top / image.height;
            float uvy1 = 1.0f - (float) src.bottom / image.height;

            var state = this._getState();
            float x, y;
            PathUtils.transformPoint(out x, out y, state.xform, (float) dst.left, (float) dst.top);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx0, uvy0));
            PathUtils.transformPoint(out x, out y, state.xform, (float) dst.left, (float) dst.bottom);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx0, uvy1));
            PathUtils.transformPoint(out x, out y, state.xform, (float) dst.right, (float) dst.bottom);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx1, uvy1));
            PathUtils.transformPoint(out x, out y, state.xform, (float) dst.right, (float) dst.top);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(uvx1, uvy0));

            var mesh = new MeshMesh(vertices, _imageTriangles, uv);
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var mat = this._getMat(paint);
            var properties = this._getMatPropsForImage(image.texture, paint);
            var isRT = image.texture is RenderTexture;
            this._getLayer().draws.Add(new RenderDraw {
                mesh = mesh,
                pass = isRT ? CanvasShaderPass.texrtPass0 : CanvasShaderPass.texPass0,
                material = mat,
                image = image, // to keep a reference to avoid GC.
                properties = properties,
            });
        }

        public void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            this.drawImageNine(image, null, center, dst, paint);
        }

        public void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(center != null);
            D.assert(dst != null);
            D.assert(paint != null);

            if (src == null) {
                src = Rect.fromLTWH(0, 0, image.width, image.height);
            }

            float x0 = (float) dst.left;
            float x3 = (float) dst.right;
            float x1 = x0 + (float) center.left / this._devicePixelRatio;
            float x2 = x3 - (float) (src.width - center.right) / this._devicePixelRatio;

            float y0 = (float) dst.top;
            float y3 = (float) dst.bottom;
            float y1 = y0 + (float) center.top / this._devicePixelRatio;
            float y2 = y3 - (float) (src.height - center.bottom) / this._devicePixelRatio;

            float tx0 = (float) src.left / image.width;
            float tx1 = (float) (src.left + center.left) / image.width;
            float tx2 = (float) (src.left + center.right) / image.width;
            float tx3 = (float) src.right / image.width;
            float ty0 = 1 - (float) src.top / image.height;
            float ty1 = 1 - (float) (src.top + center.top) / image.height;
            float ty2 = 1 - (float) (src.top + center.bottom) / image.height;
            float ty3 = 1 - (float) src.bottom / image.height;

            var vertices = new List<Vector3>(16);
            var uv = new List<Vector2>(16);

            var state = this._getState();
            float x, y;
            PathUtils.transformPoint(out x, out y, state.xform, x0, y0);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx0, ty0));
            PathUtils.transformPoint(out x, out y, state.xform, x1, y0);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx1, ty0));
            PathUtils.transformPoint(out x, out y, state.xform, x2, y0);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx2, ty0));
            PathUtils.transformPoint(out x, out y, state.xform, x3, y0);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx3, ty0));
            PathUtils.transformPoint(out x, out y, state.xform, x0, y1);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx0, ty1));
            PathUtils.transformPoint(out x, out y, state.xform, x1, y1);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx1, ty1));
            PathUtils.transformPoint(out x, out y, state.xform, x2, y1);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx2, ty1));
            PathUtils.transformPoint(out x, out y, state.xform, x3, y1);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx3, ty1));
            PathUtils.transformPoint(out x, out y, state.xform, x0, y2);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx0, ty2));
            PathUtils.transformPoint(out x, out y, state.xform, x1, y2);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx1, ty2));
            PathUtils.transformPoint(out x, out y, state.xform, x2, y2);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx2, ty2));
            PathUtils.transformPoint(out x, out y, state.xform, x3, y2);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx3, ty2));
            PathUtils.transformPoint(out x, out y, state.xform, x0, y3);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx0, ty3));
            PathUtils.transformPoint(out x, out y, state.xform, x1, y3);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx1, ty3));
            PathUtils.transformPoint(out x, out y, state.xform, x2, y3);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx2, ty3));
            PathUtils.transformPoint(out x, out y, state.xform, x3, y3);
            vertices.Add(new Vector2(x, y));
            uv.Add(new Vector2(tx3, ty3));

            var mesh = new MeshMesh(vertices, _imageNineTriangles, uv);
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var mat = this._getMat(paint);
            var properties = this._getMatPropsForImage(image.texture, paint);
            this._getLayer().draws.Add(new RenderDraw {
                mesh = mesh,
                pass = CanvasShaderPass.texPass0,
                material = mat,
                image = image, // to keep a reference to avoid GC.
                properties = properties,
            });
        }

        public void drawPicture(Picture picture) {
            this.save();

            int saveCount = 0;

            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                if (drawCmd is DrawSave) {
                    saveCount++;
                    this.save();
                }
                else if (drawCmd is DrawSaveLayer) {
                    saveCount++;
                    var drawSaveLayer = (DrawSaveLayer) drawCmd;
                    this.saveLayer(drawSaveLayer.rect, drawSaveLayer.paint);
                }
                else if (drawCmd is DrawRestore) {
                    saveCount--;
                    if (saveCount < 0) {
                        throw new Exception("unmatched save/restore in picture");
                    }

                    this.restore();
                }
                else if (drawCmd is DrawTranslate) {
                    var drawTranslate = (DrawTranslate) drawCmd;
                    this.translate(drawTranslate.dx, drawTranslate.dy);
                }
                else if (drawCmd is DrawScale) {
                    var drawScale = (DrawScale) drawCmd;
                    this.scale(drawScale.sx, drawScale.sy);
                }
                else if (drawCmd is DrawRotate) {
                    var drawRotate = (DrawRotate) drawCmd;
                    this.rotate(drawRotate.radians, drawRotate.offset);
                }
                else if (drawCmd is DrawSkew) {
                    var drawSkew = (DrawSkew) drawCmd;
                    this.skew(drawSkew.sx, drawSkew.sy);
                }
                else if (drawCmd is DrawConcat) {
                    var drawConcat = (DrawConcat) drawCmd;
                    this.concat(drawConcat.matrix);
                }
                else if (drawCmd is DrawResetMatrix) {
                    this.resetMatrix();
                }
                else if (drawCmd is DrawSetMatrix) {
                    var drawSetMatrix = (DrawSetMatrix) drawCmd;
                    this.setMatrix(drawSetMatrix.matrix);
                }
                else if (drawCmd is DrawClipRect) {
                    var drawClipRect = (DrawClipRect) drawCmd;
                    this.clipRect(drawClipRect.rect);
                }
                else if (drawCmd is DrawClipRRect) {
                    var drawClipRRect = (DrawClipRRect) drawCmd;
                    this.clipRRect(drawClipRRect.rrect);
                }
                else if (drawCmd is DrawClipPath) {
                    var drawClipPath = (DrawClipPath) drawCmd;
                    this.clipPath(drawClipPath.path);
                }
                else if (drawCmd is DrawPath) {
                    var drawPath = (DrawPath) drawCmd;
                    this.drawPath(drawPath.path, drawPath.paint);
                }
                else if (drawCmd is DrawImage) {
                    var drawImage = (DrawImage) drawCmd;
                    this.drawImage(drawImage.image, drawImage.offset, drawImage.paint);
                }
                else if (drawCmd is DrawImageRect) {
                    var drawImageRect = (DrawImageRect) drawCmd;
                    this.drawImageRect(drawImageRect.image, drawImageRect.src, drawImageRect.dst, drawImageRect.paint);
                }
                else if (drawCmd is DrawImageNine) {
                    var drawImageNine = (DrawImageNine) drawCmd;
                    this.drawImageNine(drawImageNine.image, drawImageNine.src, drawImageNine.center, drawImageNine.dst,
                        drawImageNine.paint);
                }
                else if (drawCmd is DrawPicture) {
                    var drawPicture = (DrawPicture) drawCmd;
                    this.drawPicture(drawPicture.picture);
                }
                else if (drawCmd is DrawTextBlob) {
                    var drawTextBlob = (DrawTextBlob) drawCmd;
                    this.drawTextBlob(drawTextBlob.textBlob, drawTextBlob.offset, drawTextBlob.paint);
                }
                else {
                    throw new Exception("unknown drawCmd: " + drawCmd);
                }
            }

            if (saveCount != 0) {
                throw new Exception("unmatched save/restore in picture");
            }

            this.restore();
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            var state = this._getState();

            var xform = new float[6];
            XformUtils.transformTranslate(xform, (float) offset.dx, (float) offset.dy);
            XformUtils.transformMultiply(xform, state.xform); // xform = state.xform * xform

            var scale = XformUtils.getAverageScale(xform) * this._devicePixelRatio;            
            var mesh = MeshGenerator.generateMesh(textBlob, scale)?.transform(xform);

            if (mesh == null) {
                return;
            }
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var mat = this._getMat(paint);
            var font = FontManager.instance.getOrCreate(textBlob.style.fontFamily).font;
            var properties = this._getMatPropsForImage(font.material.mainTexture, paint);
            this._getLayer().draws.Add(new RenderDraw {
                mesh = mesh,
                pass = CanvasShaderPass.texfontPass0,
                material = mat,
                properties = properties,
            });
        }

        public void flush() {
            if (this._saveCount > 0) {
                throw new Exception("unmatched save/restore");
            }

            D.assert(this._layers.Count == 1);
            D.assert(this._layers[0].states.Count == 1);

            var layer = this._getLayer();
            if (layer.draws.Count == 0) {
                D.assert(layer.layers.Count == 0);
                return;
            }

            using (var cmdBuf = new CommandBuffer()) {
                cmdBuf.name = "CommandBufferCanvas";
                this._drawLayer(layer, cmdBuf);
                Graphics.ExecuteCommandBuffer(cmdBuf);
            }

            this._clearLayer(layer);
        }

        public void reset() {
            foreach (var layer in this._layers) {
                this._clearLayer(layer);
            }

            this._saveCount = 0;

            RenderLayer firstLayer;
            if (this._layers.Count == 0) {
                var bounds = Rect.fromLTWH(0, 0,
                    this._renderTexture.width / this._devicePixelRatio,
                    this._renderTexture.height / this._devicePixelRatio);
                firstLayer = new RenderLayer {
                    width = this._renderTexture.width,
                    height = this._renderTexture.height,
                    layerBounds = bounds,
                };
            }
            else {
                D.assert(this._layers.Count > 0);
                firstLayer = this._layers[0];
                firstLayer = new RenderLayer {
                    width = firstLayer.width,
                    height = firstLayer.height,
                    layerBounds = firstLayer.layerBounds,
                };
            }

            this._layers.Clear();
            this._layers.Add(firstLayer);
        }

        void _drawLayer(RenderLayer layer, CommandBuffer cmdBuf) {
            foreach (var subLayer in layer.layers) {
                cmdBuf.GetTemporaryRT(subLayer.rtID, new RenderTextureDescriptor(
                    subLayer.width, subLayer.height,
                    RenderTextureFormat.Default, 24) {
                    msaaSamples = QualitySettings.antiAliasing,
                    useMipMap = false,
                    autoGenerateMips = false,
                }, FilterMode.Bilinear);
                this._drawLayer(subLayer, cmdBuf);
            }

            if (layer.rtID == 0) {
                cmdBuf.SetRenderTarget(this._renderTexture,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmdBuf.ClearRenderTarget(true, true, UnityEngine.Color.clear);
            }
            else {
                cmdBuf.SetRenderTarget(layer.rtID,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmdBuf.ClearRenderTarget(true, true, UnityEngine.Color.clear);
            }

            foreach (var draw in layer.draws) {
                draw.onExecute(this, cmdBuf);
            }

            foreach (var subLayer in layer.layers) {
                cmdBuf.ReleaseTemporaryRT(subLayer.rtID);
            }
        }

        void _clearLayer(RenderLayer layer) {
            foreach (var draw in layer.draws) {
                draw.onDestroy(this);
            }

            layer.draws.Clear();

            foreach (var subLayer in layer.layers) {
                this._clearLayer(subLayer);
            }

            layer.layers.Clear();
        }

        void _setLastClipGenId(uint clipGenId, Rect clipBounds) {
            var layer = this._getLayer();
            layer.lastClipGenId = clipGenId;
            layer.lastClipBounds = clipBounds;
        }

        bool _mustRenderClip(uint clipGenId, Rect clipBounds) {
            var layer = this._getLayer();
            return layer.lastClipGenId != clipGenId || layer.lastClipBounds != clipBounds;
        }

        class State {
            public float[] xform = {1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f};

            public State copy() {
                return new State {
                    xform = (float[]) this.xform.Clone(),
                };
            }
        }

        class RenderLayer {
            public int rtID;
            public int width;
            public int height;
            public Rect layerBounds;
            public Paint layerPaint;
            public readonly List<RenderCmd> draws = new List<RenderCmd>();
            public readonly List<RenderLayer> layers = new List<RenderLayer>();
            public readonly List<State> states = new List<State> {new State()};
            public readonly ClipStack clipStack = new ClipStack();
            public uint lastClipGenId;
            public Rect lastClipBounds;
        }

        interface RenderCmd {
            void onExecute(CommandBufferCanvas canvas, CommandBuffer cmdBuf);
            void onDestroy(CommandBufferCanvas canvas);
        }

        class RenderDraw : RenderCmd {
            public MeshMesh mesh;
            public int pass;
            public MaterialPropertyBlock properties;
            public RenderLayer layer;
            public Material material;
            public Image image; // just to keep a reference to avoid GC.
            public Mesh meshObj;

            public void onExecute(CommandBufferCanvas canvas, CommandBuffer cmdBuf) {
                if (this.layer != null) {
                    cmdBuf.SetGlobalTexture("_tex", this.layer.rtID);
                }

                if (!this.meshObj) {
                    this.meshObj = canvas._meshPool.getMesh();

                    // clear triangles first in order to bypass validation in SetVertices.
                    this.meshObj.SetTriangles((int[]) null, 0, false);

                    this.meshObj.SetVertices(this.mesh.vertices);
                    this.meshObj.SetTriangles(this.mesh.triangles, 0, false);
                    this.meshObj.SetUVs(0, this.mesh.uv);
                }

                cmdBuf.DrawMesh(this.meshObj, Matrix4x4.identity, this.material, 0, this.pass, this.properties);
                if (this.layer != null) {
                    cmdBuf.SetGlobalTexture("_tex", BuiltinRenderTextureType.None);
                }
            }

            public void onDestroy(CommandBufferCanvas canvas) {
                if (this.meshObj) {
                    canvas._meshPool.returnMesh(this.meshObj);
                    this.meshObj = null;
                }
            }
        }

        class RenderScissor : RenderCmd {
            public Rect deviceScissor;

            public void onExecute(CommandBufferCanvas canvas, CommandBuffer cmdBuf) {
                cmdBuf.EnableScissorRect(this.deviceScissor.toRect());
            }

            public void onDestroy(CommandBufferCanvas canvas) {
            }
        }
    }

    static class CanvasShaderPass {
        public const int fillPass0 = 0;
        public const int fillPass1 = 1;
        public const int convexFill = 2;
        public const int strokePass0 = 3;
        public const int strokePass1 = 4;
        public const int texPass0 = 5;
        public const int texrtPass0 = 6;
        public const int stencilClear = 7;
        public const int stencilIntersect0 = 8;
        public const int stencilIntersect1 = 9;
        public const int texfontPass0 = 10;
        public const int mfPass0 = 11;
    }

    static class XformUtils {
        public static void transformCopy(float[] t, float[] s) {
            t[0] = s[0];
            t[1] = s[1];
            t[2] = s[2];
            t[3] = s[3];
            t[4] = s[4];
            t[5] = s[5];
        }

        public static void transformIdentity(float[] t) {
            t[0] = 1.0f;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = 1.0f;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        public static void transformTranslate(float[] t, float tx, float ty) {
            t[0] = 1.0f;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = 1.0f;
            t[4] = tx;
            t[5] = ty;
        }

        public static void transformScale(float[] t, float sx, float sy) {
            t[0] = sx;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = sy;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        public static void transformRotate(float[] t, float a) {
            float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            t[0] = cs;
            t[1] = sn;
            t[2] = -sn;
            t[3] = cs;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        public static void transformSkew(float[] t, float sx, float sy) {
            t[0] = 1.0f;
            t[1] = Mathf.Tan(sy);
            t[2] = Mathf.Tan(sx);
            t[3] = 1.0f;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        // t = s * t;
        public static void transformMultiply(float[] t, float[] s) {
            float t0 = t[0] * s[0] + t[1] * s[2];
            float t2 = t[2] * s[0] + t[3] * s[2];
            float t4 = t[4] * s[0] + t[5] * s[2] + s[4];
            t[1] = t[0] * s[1] + t[1] * s[3];
            t[3] = t[2] * s[1] + t[3] * s[3];
            t[5] = t[4] * s[1] + t[5] * s[3] + s[5];
            t[0] = t0;
            t[2] = t2;
            t[4] = t4;
        }

        // t = t * s;
        public static void transformPremultiply(float[] t, float[] s) {
            float[] s2 = {s[0], s[1], s[2], s[3], s[4], s[5]};
            transformMultiply(s2, t);
            t[0] = s2[0];
            t[1] = s2[1];
            t[2] = s2[2];
            t[3] = s2[3];
            t[4] = s2[4];
            t[5] = s2[5];
        }

        public static int transformInverse(float[] inv, float[] t) {
            double det = (double) t[0] * t[3] - (double) t[2] * t[1];
            if (det > -1e-6 && det < 1e-6) {
                transformIdentity(inv);
                return 0;
            }

            double invdet = 1.0 / det;
            inv[0] = (float) (t[3] * invdet);
            inv[2] = (float) (-t[2] * invdet);
            inv[4] = (float) (((double) t[2] * t[5] - (double) t[3] * t[4]) * invdet);
            inv[1] = (float) (-t[1] * invdet);
            inv[3] = (float) (t[0] * invdet);
            inv[5] = (float) (((double) t[1] * t[4] - (double) t[0] * t[5]) * invdet);
            return 1;
        }

        public static float getAverageScale(float[] t) {
            return (getScaleX(t) + getScaleY(t)) * 0.5f;
        }

        public static float getMaxScale(float[] t) {
            return Mathf.Max(getScaleX(t), getScaleY(t));
        }

        public static float getScaleX(float[] t) {
            return Mathf.Sqrt(t[0] * t[0] + t[2] * t[2]);
        }

        public static float getScaleY(float[] t) {
            return Mathf.Sqrt(t[1] * t[1] + t[3] * t[3]);
        }

        public static float mapRadius(float[] t, float radius) {
            // mapped point of (radius, 0) and (0, radius)
            var x0 = radius * t[0] + t[4];
            var y0 = radius * t[1] + t[5];
            var x1 = radius * t[2] + t[4];
            var y1 = radius * t[3] + t[5];

            // geometric mean of len0 and len1.
            return Mathf.Sqrt(Mathf.Sqrt((x0 * x0 + y0 * y0) * (x1 * x1 + y1 * y1)));
        }

        public static void transformPoint(out float dx, out float dy, float[] t, float sx, float sy) {
            dx = sx * t[0] + sy * t[2] + t[4];
            dy = sx * t[1] + sy * t[3] + t[5];
        }

        public static float[] fromMatrix3(Matrix3 matrix) {
            return new[] {
                matrix.getScaleX(), matrix.getSkewY(),
                matrix.getSkewX(), matrix.getScaleY(),
                matrix.getTranslateX(), matrix.getTranslateY(),
            };
        }

        public static Matrix3 toMatrix3(float[] xform) {
            return Matrix3.makeAll(
                xform[0], xform[2], xform[4],
                xform[1], xform[3], xform[5],
                0, 0, 1
            );
        }
    }

    static class BlurUtils {
        static readonly Dictionary<_GaussianKernelKey, float[]> _gaussianKernels
            = new Dictionary<_GaussianKernelKey, float[]>();

        public static float[] get1DGaussianKernel(float gaussianSigma, int radius) {
            var width = 2 * radius + 1;
            D.assert(width <= 25);

            var key = new _GaussianKernelKey(gaussianSigma, radius);
            return _gaussianKernels.putIfAbsent(key, () => {
                var kernel = new float[25];
                float twoSigmaSqrd = 2.0f * gaussianSigma * gaussianSigma;

                if (ScalarUtils.ScalarNearlyZero(twoSigmaSqrd)) {
                    for (int i = 0; i < width; ++i) {
                        kernel[i] = 0.0f;
                    }

                    return kernel;
                }

                float denom = 1.0f / twoSigmaSqrd;

                float sum = 0.0f;
                for (int i = 0; i < width; ++i) {
                    float x = i - radius;
                    // Note that the constant term (1/(sqrt(2*pi*sigma^2)) of the Gaussian
                    // is dropped here, since we renormalize the kernel below.
                    kernel[i] = Mathf.Exp(-x * x * denom);
                    sum += kernel[i];
                }

                // Normalize the kernel
                float scale = 1.0f / sum;
                for (int i = 0; i < width; ++i) {
                    kernel[i] *= scale;
                }

                return kernel;
            });
        }

        public static float adjustSigma(float sigma, out int scaleFactor, out int radius) {
            scaleFactor = 1;

            const int maxTextureSize = 16384;
            const float MAX_BLUR_SIGMA = 4.0f;

            while (sigma > MAX_BLUR_SIGMA) {
                scaleFactor *= 2;
                sigma *= 0.5f;

                if (scaleFactor > maxTextureSize) {
                    scaleFactor = maxTextureSize;
                    sigma = MAX_BLUR_SIGMA;
                }
            }

            radius = Mathf.CeilToInt(sigma * 3.0f);
            D.assert(radius <= 3 * MAX_BLUR_SIGMA);
            return sigma;
        }

        class _GaussianKernelKey : IEquatable<_GaussianKernelKey> {
            public readonly float gaussianSigma;
            public readonly int radius;

            public _GaussianKernelKey(float gaussianSigma, int radius) {
                this.gaussianSigma = gaussianSigma;
                this.radius = radius;
            }

            public bool Equals(_GaussianKernelKey other) {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return this.gaussianSigma.Equals(other.gaussianSigma) &&
                       this.radius == other.radius;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != this.GetType()) {
                    return false;
                }

                return this.Equals((_GaussianKernelKey) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = this.gaussianSigma.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.radius;
                    return hashCode;
                }
            }

            public static bool operator ==(_GaussianKernelKey left, _GaussianKernelKey right) {
                return Equals(left, right);
            }

            public static bool operator !=(_GaussianKernelKey left, _GaussianKernelKey right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return $"_GaussianKernelKey(gaussianSigma: {this.gaussianSigma:F1}, radius: {this.radius})";
            }
        }
    }
}