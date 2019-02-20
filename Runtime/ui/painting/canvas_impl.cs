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
                (float) bounds.width * state.scale * this._devicePixelRatio);
            textureWidth = Mathf.Max(textureWidth, 1);

            var textureHeight = Mathf.CeilToInt(
                (float) bounds.height * state.scale * this._devicePixelRatio);
            textureHeight = Mathf.Max(textureHeight, 1);

            var parentLayer = this._getLayer();
            var layer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = bounds,
                layerPaint = paint,
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

            var mesh = ImageMeshGenerator.imageMesh(state.matrix, Rect.one, layer.layerBounds);

            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var renderDraw = CanvasShader.texRT(this._getLayer(), layer.layerPaint, mesh, layer);
            this._getLayer().draws.Add(renderDraw);
        }

        public void translate(double dx, double dy) {
            var state = this._getState();
            var matrix = Matrix3.makeTrans((float) dx, (float) dy);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        public void scale(double sx, double? sy = null) {
            var state = this._getState();
            var matrix = Matrix3.makeScale((float) sx, (float) (sy ?? sx));
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        public void rotate(double radians, Offset offset = null) {
            var state = this._getState();

            if (offset == null) {
                var matrix = Matrix3.makeRotate((float) radians);
                matrix.postConcat(state.matrix);
                state.matrix = matrix;
            } else {
                var matrix = Matrix3.makeRotate((float) radians, (float) offset.dx, (float) offset.dy);
                matrix.postConcat(state.matrix);
                state.matrix = matrix;
            }
        }

        public void skew(double sx, double sy) {
            var state = this._getState();
            var matrix = Matrix3.makeSkew((float) sx, (float) sy);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        public void concat(Matrix3 matrix) {
            var state = this._getState();
            matrix = new Matrix3(matrix);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        public Matrix3 getTotalMatrix() {
            var state = this._getState();
            return state.matrix;
        }

        public void resetMatrix() {
            var state = this._getState();
            state.matrix = Matrix3.I();
        }

        public void setMatrix(Matrix3 matrix) {
            var state = this._getState();
            state.matrix = new Matrix3(matrix);
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
            layer.clipStack.clipPath(path, state.matrix, state.scale * this._devicePixelRatio);
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

        void _tryAddScissor(RenderLayer layer, Rect scissor) {
            if (scissor == layer.lastScissor) {
                return;
            }

            layer.draws.Add(new RenderScissor {
                deviceScissor = scissor,
            });
            layer.lastScissor = scissor;
        }
        
        bool _applyClip(Rect queryBounds) {
            var layer = this._getLayer();
            var layerBounds = layer.layerBounds;
            ReducedClip reducedClip = new ReducedClip(layer.clipStack, layerBounds, queryBounds);
            if (reducedClip.isEmpty()) {
                return false;
            }

            var scissor = reducedClip.scissor;
            var physicalRect = Rect.fromLTRB(0, 0, layer.width, layer.height);

            if (scissor == layerBounds) {
                this._tryAddScissor(layer, null);
            } else {
                var deviceScissor = Rect.fromLTRB(
                    scissor.left - layerBounds.left, layerBounds.bottom - scissor.bottom,
                    scissor.right - layerBounds.left, layerBounds.bottom - scissor.top
                ).scale(layer.width / layerBounds.width, layer.height / layerBounds.height);
                deviceScissor = deviceScissor.roundIn();
                deviceScissor = deviceScissor.intersect(physicalRect);

                if (deviceScissor.isEmpty) {
                    return false;
                }
                
                this._tryAddScissor(layer, deviceScissor);
            }

            var maskGenID = reducedClip.maskGenID();
            if (this._mustRenderClip(maskGenID, reducedClip.scissor)) {
                if (maskGenID == ClipStack.wideOpenGenID) {
                    layer.ignoreClip = true;
                } else {
                    layer.ignoreClip = false;

                    var boundsMesh = new MeshMesh(reducedClip.scissor);
                    layer.draws.Add(CanvasShader.stencilClear(layer, boundsMesh));

                    foreach (var maskElement in reducedClip.maskElements) {
                        layer.draws.Add(CanvasShader.stencil0(layer, maskElement.mesh));
                        layer.draws.Add(CanvasShader.stencil1(layer, boundsMesh));
                    }
                }

                this._setLastClipGenId(maskGenID, reducedClip.scissor);
            }

            return true;
        }

        RenderLayer _createMaskLayer(RenderLayer parentLayer, Rect maskBounds, Action<Paint> drawCallback, Paint paint) {
            var textureWidth = Mathf.CeilToInt((float) maskBounds.width * this._devicePixelRatio);
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
            maskState.matrix = parentState.matrix;

            drawCallback(Paint.shapeOnly(paint));

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

            var blurMesh = ImageMeshGenerator.imageMesh(null, Rect.one, maskLayer.layerBounds);

            var kernel = BlurUtils.get1DGaussianKernel(sigma, radius);

            blurXLayer.draws.Add(CanvasShader.maskFilter(
                blurXLayer, blurMesh, maskLayer,
                radius, new Vector2(1f / textureWidth, 0), kernel));

            blurYLayer.draws.Add(CanvasShader.maskFilter(
                blurYLayer, blurMesh, blurXLayer,
                radius, new Vector2(0, -1f / textureHeight), kernel));

            return blurYLayer;
        }

        void _drawWithMaskFilter(Rect meshBounds, Action<Paint> drawAction, Paint paint, MaskFilter maskFilter) {
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
            float sigma = state.scale * (float) maskFilter.sigma;
            if (sigma <= 0) {
                return;
            }

            float sigma3 = 3 * sigma;
            var maskBounds = meshBounds.inflate(sigma3);
            maskBounds = maskBounds.intersect(clipBounds.inflate(sigma3));
            if (maskBounds.isEmpty) {
                return;
            }

            var maskLayer = this._createMaskLayer(layer, maskBounds, drawAction, paint);

            var blurLayer = this._createBlurLayer(maskLayer, sigma, layer);

            var blurMesh = ImageMeshGenerator.imageMesh(null, Rect.one, maskBounds);
            if (!this._applyClip(blurMesh.bounds)) {
                return;
            }

            layer.draws.Add(CanvasShader.texRT(layer, paint, blurMesh, blurLayer));
        }

        public void drawPath(Path path, Paint paint) {
            D.assert(path != null);
            D.assert(paint != null);
            
            if (paint.style == PaintingStyle.fill) {
                var state = this._getState();
                var cache = path.flatten(state.scale * this._devicePixelRatio);

                bool convex;
                var mesh = cache.getFillMesh(out convex).transform(state.matrix);
                
                Action<Paint> drawMesh = (Paint p) => {
                    if (!this._applyClip(mesh.bounds)) {
                        return;
                    }

                    var layer = this._getLayer();
                    if (convex) {
                        layer.draws.Add(CanvasShader.convexFill(layer, p, mesh));
                    }
                    else {
                        layer.draws.Add(CanvasShader.fill0(layer, mesh));
                        layer.draws.Add(CanvasShader.fill1(layer, p, mesh.boundsMesh));
                    }
                };

                if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                    this._drawWithMaskFilter(mesh.bounds, drawMesh, paint, paint.maskFilter);
                    return;
                }
                
                drawMesh(paint);
            }
            else {
                var state = this._getState();
                float strokeWidth = ((float) paint.strokeWidth * state.scale).clamp(0, 200.0f);
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

                var cache = path.flatten(state.scale * this._devicePixelRatio);
                var mesh = cache.getStrokeMesh(
                    strokeWidth / state.scale * 0.5f,
                    paint.strokeCap,
                    paint.strokeJoin,
                    (float) paint.strokeMiterLimit).transform(state.matrix);

                Action<Paint> drawMesh = (Paint p) => {
                    if (!this._applyClip(mesh.bounds)) {
                        return;
                    }

                    var layer = this._getLayer();
                    layer.draws.Add(CanvasShader.stroke0(layer, p, alpha, mesh));
                    layer.draws.Add(CanvasShader.stroke1(layer, mesh));
                };
                
                if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                    this._drawWithMaskFilter(mesh.bounds, drawMesh, paint, paint.maskFilter);
                    return;
                }
                
                drawMesh(paint);
            }
        }

        public void drawImage(Image image, Offset offset, Paint paint) {
            D.assert(image != null);
            D.assert(offset != null);
            D.assert(paint != null);

            this.drawImageRect(image,
                null,
                Rect.fromLTWH(
                    offset.dx, offset.dy,
                    image.width / this._devicePixelRatio,
                    image.height / this._devicePixelRatio),
                paint);
        }

        public void drawImageRect(Image image, Rect dst, Paint paint) {
            this.drawImageRect(image, null, dst, paint);
        }

        public void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(dst != null);
            D.assert(paint != null);

            if (src == null) {
                src = Rect.one;
            } else {
                src = src.scale(1f / image.width, 1f / image.height);
            }

            var state = this._getState();
            var mesh = ImageMeshGenerator.imageMesh(state.matrix, src, dst);
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var layer = this._getLayer();
            layer.draws.Add(CanvasShader.tex(layer, paint, mesh, image));
        }

        public void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            this.drawImageNine(image, null, center, dst, paint);
        }

        public void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(center != null);
            D.assert(dst != null);
            D.assert(paint != null);

            var scaleX = 1f / image.width;
            var scaleY = 1f / image.height;
            if (src == null) {
                src = Rect.one;
            } else {
                src = src.scale(scaleX, scaleY);                
            }

            center = center.scale(scaleX, scaleY);

            var state = this._getState();

            var mesh = ImageMeshGenerator.imageNineMesh(state.matrix, src, center, image.width, image.height, dst);
            
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var layer = this._getLayer();
            layer.draws.Add(CanvasShader.tex(layer, paint, mesh, image));
        }

        public void drawPicture(Picture picture) {
            this.save();

            int saveCount = 0;

            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                switch (drawCmd) {
                    case DrawSave _:
                        saveCount++;
                        this.save();
                        break;
                    case DrawSaveLayer cmd: {
                        saveCount++;
                        this.saveLayer(cmd.rect, cmd.paint);
                        break;
                    }
                    case DrawRestore _: {
                        saveCount--;
                        if (saveCount < 0) {
                            throw new Exception("unmatched save/restore in picture");
                        }

                        this.restore();
                        break;
                    }
                    case DrawTranslate cmd: {
                        this.translate(cmd.dx, cmd.dy);
                        break;
                    }
                    case DrawScale cmd: {
                        this.scale(cmd.sx, cmd.sy);
                        break;
                    }
                    case DrawRotate cmd: {
                        this.rotate(cmd.radians, cmd.offset);
                        break;
                    }
                    case DrawSkew cmd: {
                        this.skew(cmd.sx, cmd.sy);
                        break;
                    }
                    case DrawConcat cmd: {
                        this.concat(cmd.matrix);
                        break;
                    }
                    case DrawResetMatrix _:
                        this.resetMatrix();
                        break;
                    case DrawSetMatrix cmd: {
                        this.setMatrix(cmd.matrix);
                        break;
                    }
                    case DrawClipRect cmd: {
                        this.clipRect(cmd.rect);
                        break;
                    }
                    case DrawClipRRect cmd: {
                        this.clipRRect(cmd.rrect);
                        break;
                    }
                    case DrawClipPath cmd: {
                        this.clipPath(cmd.path);
                        break;
                    }
                    case DrawPath cmd: {
                        this.drawPath(cmd.path, cmd.paint);
                        break;
                    }
                    case DrawImage cmd: {
                        this.drawImage(cmd.image, cmd.offset, cmd.paint);
                        break;
                    }
                    case DrawImageRect cmd: {
                        this.drawImageRect(cmd.image, cmd.src, cmd.dst, cmd.paint);
                        break;
                    }
                    case DrawImageNine cmd: {
                        this.drawImageNine(cmd.image, cmd.src, cmd.center, cmd.dst, cmd.paint);
                        break;
                    }
                    case DrawPicture cmd: {
                        this.drawPicture(cmd.picture);
                        break;
                    }
                    case DrawTextBlob cmd: {
                        this.drawTextBlob(cmd.textBlob, cmd.offset, cmd.paint);
                        break;
                    }
                    default:
                        throw new Exception("unknown drawCmd: " + drawCmd);
                }
            }

            if (saveCount != 0) {
                throw new Exception("unmatched save/restore in picture");
            }

            this.restore();
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            D.assert(textBlob != null);
            D.assert(offset != null);
            D.assert(paint != null);
            
            var state = this._getState();
            var scale = state.scale * this._devicePixelRatio;
            
            var matrix = new Matrix3(state.matrix);
            matrix.preTranslate((float) offset.dx, (float) offset.dy);            
            var mesh = MeshGenerator.generateMesh(textBlob, scale)?.transform(matrix);
            if (mesh == null) {
                return;
            }
            
            var font = FontManager.instance.getOrCreate(textBlob.style.fontFamily).font;
            var tex = font.material.mainTexture;

            Action<Paint> drawMesh = (Paint p) => {
                if (!this._applyClip(mesh.bounds)) {
                    return;
                }

                var layer = this._getLayer();
                layer.draws.Add(CanvasShader.texAlpha(layer, p, mesh, tex));
            };

            if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                this._drawWithMaskFilter(mesh.bounds, drawMesh, paint, paint.maskFilter);
                return;
            }

            drawMesh(paint);
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
                var desc = new RenderTextureDescriptor(
                    subLayer.width, subLayer.height,
                    RenderTextureFormat.Default, 24) {
                    useMipMap = false,
                    autoGenerateMips = false,
                };
                
                if (QualitySettings.antiAliasing != 0) {
                    desc.msaaSamples = QualitySettings.antiAliasing;
                }
                
                cmdBuf.GetTemporaryRT(subLayer.rtID, desc, FilterMode.Bilinear);
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

            foreach (var cmdObj in layer.draws) {
                switch (cmdObj) {
                    case RenderDraw cmd:
                        if (cmd.layer != null) {
                            cmdBuf.SetGlobalTexture(RenderDraw.texId, cmd.layer.rtID);
                        }

                        D.assert(cmd.meshObj == null);
                        cmd.meshObj = this._meshPool.getMesh();
                        cmd.meshObjCreated = true;

                        // clear triangles first in order to bypass validation in SetVertices.
                        cmd.meshObj.SetTriangles((int[]) null, 0, false);

                        cmd.meshObj.SetVertices(cmd.mesh.vertices);
                        cmd.meshObj.SetTriangles(cmd.mesh.triangles, 0, false);
                        cmd.meshObj.SetUVs(0, cmd.mesh.uv);

                        if (cmd.mesh.matrix == null) {
                            cmd.properties.SetFloatArray(RenderDraw.matId, RenderDraw.idMat3.fMat);
                        } else {
                            cmd.properties.SetFloatArray(RenderDraw.matId, cmd.mesh.matrix.fMat);
                        }

                        cmdBuf.DrawMesh(cmd.meshObj, RenderDraw.idMat, cmd.material, 0, cmd.pass, cmd.properties);
                        if (cmd.layer != null) {
                            cmdBuf.SetGlobalTexture(RenderDraw.texId, BuiltinRenderTextureType.None);
                        }
                        break;
                    case RenderScissor cmd:
                        if (cmd.deviceScissor == null) {
                            cmdBuf.DisableScissorRect();
                        } else {
                            cmdBuf.EnableScissorRect(cmd.deviceScissor.toRect());
                        }
                        break;
                }
            }

            foreach (var subLayer in layer.layers) {
                cmdBuf.ReleaseTemporaryRT(subLayer.rtID);
            }
        }

        void _clearLayer(RenderLayer layer) {
            for (var index = 0; index < layer.draws.Count; index++) {
                var cmdObj = layer.draws[index];
                switch (cmdObj) {
                    case RenderDraw cmd:
                        if (cmd.meshObjCreated) {
                            this._meshPool.returnMesh(cmd.meshObj);
                            cmd.meshObj = null;
                            cmd.meshObjCreated = false;
                        }
                        break;
                }
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

        internal class State {
            static readonly Matrix3 _id = Matrix3.I();
            
            Matrix3 _matrix;
            float? _scale;

            public State(Matrix3 matrix = null, float? scale = null) {
                this._matrix = matrix ?? _id;
                this._scale = scale;
            }
            
            public Matrix3 matrix {
                get { return this._matrix; }
                set {
                    this._matrix = value;
                    this._scale = null;
                }
            }

            public float scale {
                get {
                    if (this._scale == null) {
                        this._scale = XformUtils.getScale(this._matrix);
                    }
                    return this._scale.Value;
                }
            }

            public State copy() {
                return new State(this._matrix, this._scale);
            }
        }

        internal class RenderLayer {
            public int rtID;
            public int width;
            public int height;
            public Rect layerBounds;
            public Paint layerPaint;
            public readonly List<object> draws = new List<object>();
            public readonly List<RenderLayer> layers = new List<RenderLayer>();
            public readonly List<State> states = new List<State> {new State()};
            public readonly ClipStack clipStack = new ClipStack();
            public uint lastClipGenId;
            public Rect lastClipBounds;
            public Rect lastScissor;
            public bool ignoreClip;

            Vector4? _viewport;

            public Vector4 viewport {
                get {
                    if (!this._viewport.HasValue) {
                        this._viewport = new Vector4(
                            (float) this.layerBounds.left,
                            (float) this.layerBounds.top,
                            (float) this.layerBounds.width,
                            (float) this.layerBounds.height);
                    }
                    return this._viewport.Value;
                }
            }
        }

        internal class RenderDraw {
            public MeshMesh mesh;
            public int pass;
            public MaterialPropertyBlock properties;
            public RenderLayer layer;
            public Material material;
            public Image image; // just to keep a reference to avoid GC.
            public Mesh meshObj;
            public bool meshObjCreated;

            public static readonly Matrix4x4 idMat = Matrix4x4.identity;
            public static readonly Matrix3 idMat3 = Matrix3.I();
            public static readonly int texId = Shader.PropertyToID("_tex");
            public static readonly int matId = Shader.PropertyToID("_mat");
        }

        internal class RenderScissor {
            public Rect deviceScissor;
        }
    }


    static class XformUtils {
        public static float getAverageScale(Matrix3 matrix) {
            return (getScaleX(matrix) + getScaleY(matrix)) * 0.5f;
        }

        public static float getMaxScale(Matrix3 matrix) {
            return Mathf.Max(getScaleX(matrix), getScaleY(matrix));
        }

        public static float getScaleX(Matrix3 matrix) {
            // ignore perspective parameters for now.
            if (matrix.isIdentity()) {
                return 1.0f;
            }

            if (matrix.getSkewY() == 0) {
                return matrix.getScaleX();
            }
            
            var x = matrix.getScaleX();
            var y = matrix.getSkewY();
            
            return Mathf.Sqrt(x * x + y * y);
        }

        public static float getScaleY(Matrix3 matrix) {
            // ignore perspective parameters for now.
            if (matrix.isIdentity()) {
                return 1.0f;
            }

            if (matrix.getSkewX() == 0) {
                return matrix.getScaleY();
            }

            var x = matrix.getSkewX();
            var y = matrix.getScaleY();

            return Mathf.Sqrt(x * x + y * y);
        }

        public static float getScale(Matrix3 matrix) {
            var scaleX = getScaleX(matrix);
            var scaleY = getScaleY(matrix);

            if (scaleX == 1.0) {
                return scaleY;
            }

            if (scaleY == 1.0) {
                return scaleX;
            }

            // geometric mean of len0 and len1.
            return Mathf.Sqrt(scaleX * scaleY);
        }
        
        public static float mapRadius(Matrix3 matrix, float radius) {
            return getScale(matrix) * radius;
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

    static class ImageMeshGenerator {
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

        public static MeshMesh imageMesh(Matrix3 matrix, Rect src, Rect dst) {
            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            float uvx0 = (float) src.left;
            float uvx1 = (float) src.right;
            float uvy0 = 1.0f - (float) src.top;
            float uvy1 = 1.0f - (float) src.bottom;

            vertices.Add(new Vector2((float) dst.left, (float) dst.top));
            uv.Add(new Vector2(uvx0, uvy0));
            vertices.Add(new Vector2((float) dst.left, (float) dst.bottom));
            uv.Add(new Vector2(uvx0, uvy1));
            vertices.Add(new Vector2((float) dst.right, (float) dst.bottom));
            uv.Add(new Vector2(uvx1, uvy1));
            vertices.Add(new Vector2((float) dst.right, (float) dst.top));
            uv.Add(new Vector2(uvx1, uvy0));

            return new MeshMesh(matrix, vertices, _imageTriangles, uv);
        }

        public static MeshMesh imageNineMesh(Matrix3 matrix, Rect src, Rect center, int srcWidth, int srcHeight, Rect dst) {
            float x0 = (float) dst.left;
            float x3 = (float) dst.right;
            float x1 = x0 + (float) ((center.left - src.left) * srcWidth);
            float x2 = x3 - (float) ((src.right - center.right) * srcWidth);

            float y0 = (float) dst.top;
            float y3 = (float) dst.bottom;
            float y1 = y0 + (float) ((center.top - src.top) * srcHeight);
            float y2 = y3 - (float) ((src.bottom - center.bottom) * srcHeight);

            float tx0 = (float) src.left;
            float tx1 = (float) center.left;
            float tx2 = (float) center.right;
            float tx3 = (float) src.right;
            float ty0 = 1 - (float) src.top;
            float ty1 = 1 - (float) center.top;
            float ty2 = 1 - (float) center.bottom;
            float ty3 = 1 - (float) src.bottom;

            var vertices = new List<Vector3>(16);
            var uv = new List<Vector2>(16);

            vertices.Add(new Vector2(x0, y0));
            uv.Add(new Vector2(tx0, ty0));
            vertices.Add(new Vector2(x1, y0));
            uv.Add(new Vector2(tx1, ty0));
            vertices.Add(new Vector2(x2, y0));
            uv.Add(new Vector2(tx2, ty0));
            vertices.Add(new Vector2(x3, y0));
            uv.Add(new Vector2(tx3, ty0));
            vertices.Add(new Vector2(x0, y1));
            uv.Add(new Vector2(tx0, ty1));
            vertices.Add(new Vector2(x1, y1));
            uv.Add(new Vector2(tx1, ty1));
            vertices.Add(new Vector2(x2, y1));
            uv.Add(new Vector2(tx2, ty1));
            vertices.Add(new Vector2(x3, y1));
            uv.Add(new Vector2(tx3, ty1));
            vertices.Add(new Vector2(x0, y2));
            uv.Add(new Vector2(tx0, ty2));
            vertices.Add(new Vector2(x1, y2));
            uv.Add(new Vector2(tx1, ty2));
            vertices.Add(new Vector2(x2, y2));
            uv.Add(new Vector2(tx2, ty2));
            vertices.Add(new Vector2(x3, y2));
            uv.Add(new Vector2(tx3, ty2));
            vertices.Add(new Vector2(x0, y3));
            uv.Add(new Vector2(tx0, ty3));
            vertices.Add(new Vector2(x1, y3));
            uv.Add(new Vector2(tx1, ty3));
            vertices.Add(new Vector2(x2, y3));
            uv.Add(new Vector2(tx2, ty3));
            vertices.Add(new Vector2(x3, y3));
            uv.Add(new Vector2(tx3, ty3));
            
            return new MeshMesh(matrix, vertices, _imageNineTriangles, uv);
        }        
    }
}