using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    public class PictureFlusher {
        readonly RenderTexture _renderTexture;
        readonly float _fringeWidth;
        readonly float _devicePixelRatio;
        readonly MeshPool _meshPool;
        
        readonly List<RenderLayer> _layers = new List<RenderLayer>();
        RenderLayer _currentLayer;
        Rect _lastScissor;
        
        public PictureFlusher(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool) {
            D.assert(renderTexture);
            D.assert(devicePixelRatio > 0);
            D.assert(meshPool != null);

            this._renderTexture = renderTexture;
            this._fringeWidth = 1.0f / devicePixelRatio;
            this._devicePixelRatio = devicePixelRatio;
            this._meshPool = meshPool;
        }
        
        public float getDevicePixelRatio() {
            return this._devicePixelRatio;
        }
        
        void _reset() {
            foreach (var layer in this._layers) {
                this._clearLayer(layer);
            }
            
            RenderLayer firstLayer;
            if (this._layers.Count == 0) {
                var width = this._renderTexture.width;
                var height = this._renderTexture.height;

                var bounds = Rect.fromLTWH(0, 0,
                    width * this._fringeWidth,
                    height * this._fringeWidth);

                firstLayer = new RenderLayer {
                    width = width,
                    height = height,
                    layerBounds = bounds,
                };
            } else {
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
            this._currentLayer = firstLayer;
        }

        void _save() {
            var layer = this._currentLayer;
            layer.currentState = layer.currentState.copy();
            layer.states.Add(layer.currentState);
            layer.clipStack.save();
        }

        void _saveLayer(Rect bounds, Paint paint) {
            D.assert(bounds != null);
            D.assert(bounds.width > 0);
            D.assert(bounds.height > 0);
            D.assert(paint != null);

            var parentLayer = this._currentLayer;
            var state = parentLayer.currentState;
            var textureWidth = Mathf.CeilToInt(
                bounds.width * state.scale * this._devicePixelRatio);
            if (textureWidth < 1) {
                textureWidth = 1;
            }

            var textureHeight = Mathf.CeilToInt(
                bounds.height * state.scale * this._devicePixelRatio);
            if (textureHeight < 1) {
                textureHeight = 1;
            }

            var layer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = bounds,
                layerPaint = paint,
            };

            parentLayer.addLayer(layer);
            this._layers.Add(layer);
            this._currentLayer = layer;

            if (paint.backdrop != null) {
                if (paint.backdrop is _BlurImageFilter) {
                    var filter = (_BlurImageFilter) paint.backdrop;
                    if (!(filter.sigmaX == 0 && filter.sigmaY == 0)) {
                        var points = new[] {bounds.topLeft, bounds.bottomLeft, bounds.bottomRight, bounds.topRight};
                        state.matrix.mapPoints(points);

                        var parentBounds = parentLayer.layerBounds;
                        for (int i = 0; i < 4; i++) {
                            points[i] = new Offset(
                                (points[i].dx - parentBounds.left) / parentBounds.width,
                                (points[i].dy - parentBounds.top) / parentBounds.height
                            );
                        }

                        var mesh = ImageMeshGenerator.imageMesh(
                            null,
                            points[0], points[1], points[2], points[3],
                            bounds);
                        var renderDraw = CanvasShader.texRT(layer, layer.layerPaint, mesh, parentLayer);
                        layer.draws.Add(renderDraw);

                        var blurLayer = this._createBlurLayer(layer, filter.sigmaX, filter.sigmaY, layer);
                        var blurMesh = ImageMeshGenerator.imageMesh(null, Rect.one, bounds);
                        layer.draws.Add(CanvasShader.texRT(layer, paint, blurMesh, blurLayer));
                    }
                } else if (paint.backdrop is _MatrixImageFilter) {
                    var filter = (_MatrixImageFilter) paint.backdrop;
                    if (!filter.transform.isIdentity()) {
                        layer.filterMode = filter.filterMode;
                    
                        var points = new[] {bounds.topLeft, bounds.bottomLeft, bounds.bottomRight, bounds.topRight};
                        state.matrix.mapPoints(points);

                        var parentBounds = parentLayer.layerBounds;
                        for (int i = 0; i < 4; i++) {
                            points[i] = new Offset(
                                (points[i].dx - parentBounds.left) / parentBounds.width,
                                (points[i].dy - parentBounds.top) / parentBounds.height
                            );
                        }

                        var matrix = Matrix3.makeTrans(-bounds.left, -bounds.top);
                        matrix.postConcat(filter.transform);
                        matrix.postTranslate(bounds.left, bounds.top);
                        
                        var mesh = ImageMeshGenerator.imageMesh(
                            matrix,
                            points[0], points[1], points[2], points[3],
                            bounds);
                        var renderDraw = CanvasShader.texRT(layer, layer.layerPaint, mesh, parentLayer);
                        layer.draws.Add(renderDraw);
                    }
                }
            }
        }
        
        void _restore() {
            var layer = this._currentLayer;
            D.assert(layer.states.Count > 0);
            if (layer.states.Count > 1) {
                layer.states.RemoveAt(layer.states.Count - 1);
                layer.currentState = layer.states[layer.states.Count - 1];
                layer.clipStack.restore();
                return;
            }

            this._layers.RemoveAt(this._layers.Count - 1);
            var currentLayer = this._currentLayer = this._layers[this._layers.Count - 1];
            var state = currentLayer.currentState;

            var mesh = ImageMeshGenerator.imageMesh(state.matrix, Rect.one, layer.layerBounds);

            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            var renderDraw = CanvasShader.texRT(currentLayer, layer.layerPaint, mesh, layer);
            currentLayer.draws.Add(renderDraw);
        }
        
        void _translate(float dx, float dy) {
            var state = this._currentLayer.currentState;
            var matrix = Matrix3.makeTrans(dx, dy);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        void _scale(float sx, float? sy = null) {
            var state = this._currentLayer.currentState;
            var matrix = Matrix3.makeScale(sx, (sy ?? sx));
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        void _rotate(float radians, Offset offset = null) {
            var state = this._currentLayer.currentState;
            if (offset == null) {
                var matrix = Matrix3.makeRotate(radians);
                matrix.postConcat(state.matrix);
                state.matrix = matrix;
            } else {
                var matrix = Matrix3.makeRotate(radians, offset.dx, offset.dy);
                matrix.postConcat(state.matrix);
                state.matrix = matrix;
            }
        }

        void _skew(float sx, float sy) {
            var state = this._currentLayer.currentState;
            var matrix = Matrix3.makeSkew( sx, sy);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        void _concat(Matrix3 matrix) {
            var state = this._currentLayer.currentState;
            matrix = new Matrix3(matrix);
            matrix.postConcat(state.matrix);
            state.matrix = matrix;
        }

        void _resetMatrix() {
            var state = this._currentLayer.currentState;
            state.matrix = Matrix3.I();
        }

        void _setMatrix(Matrix3 matrix) {
            var state = this._currentLayer.currentState;
            state.matrix = new Matrix3(matrix);
        }

        void _clipRect(Rect rect) {
            var path = new Path();
            path.addRect(rect);
            this._clipPath(path);
        }

        void _clipRRect(RRect rrect) {
            var path = new Path();
            path.addRRect(rrect);
            this._clipPath(path);
        }

        void _clipPath(Path path) {
            var layer = this._currentLayer;
            var state = layer.currentState;
            layer.clipStack.clipPath(path, state.matrix, state.scale * this._devicePixelRatio);
        }
        
        void _tryAddScissor(RenderLayer layer, Rect scissor) {
            if (scissor == this._lastScissor) {
                return;
            }

            layer.draws.Add(new CmdScissor {
                deviceScissor = scissor,
            });
            this._lastScissor = scissor;
        }
        
        bool _applyClip(Rect queryBounds) {
            if (queryBounds == null || queryBounds.isEmpty) {
                return false;
            }

            var layer = this._currentLayer;
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
                deviceScissor = deviceScissor.roundOut();
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

                    // need to inflate a bit to make sure all area is cleared.
                    var inflatedScissor = reducedClip.scissor.inflate(this._fringeWidth);
                    var boundsMesh = new MeshMesh(inflatedScissor);
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
        
        void _setLastClipGenId(uint clipGenId, Rect clipBounds) {
            var layer = this._currentLayer;
            layer.lastClipGenId = clipGenId;
            layer.lastClipBounds = clipBounds;
        }

        bool _mustRenderClip(uint clipGenId, Rect clipBounds) {
            var layer = this._currentLayer;
            return layer.lastClipGenId != clipGenId || layer.lastClipBounds != clipBounds;
        }

        RenderLayer _createMaskLayer(RenderLayer parentLayer, Rect maskBounds, Action<Paint> drawCallback, Paint paint) {
            var textureWidth = Mathf.CeilToInt(maskBounds.width * this._devicePixelRatio);
            if (textureWidth < 1) {
                textureWidth = 1;
            }

            var textureHeight = Mathf.CeilToInt(maskBounds.height * this._devicePixelRatio);
            if (textureHeight < 1) {
                textureHeight = 1;
            }

            var maskLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskBounds,
                filterMode = FilterMode.Bilinear,
                noMSAA = true,
            };

            parentLayer.addLayer(maskLayer);
            this._layers.Add(maskLayer);
            this._currentLayer = maskLayer;

            var parentState = parentLayer.states[parentLayer.states.Count - 1];
            var maskState = maskLayer.states[maskLayer.states.Count - 1];
            maskState.matrix = parentState.matrix;

            drawCallback(Paint.shapeOnly(paint));

            var removed = this._layers.removeLast();
            D.assert(removed == maskLayer);
            this._currentLayer = this._layers[this._layers.Count - 1];

            return maskLayer;
        }

        RenderLayer _createBlurLayer(RenderLayer maskLayer, float sigmaX, float sigmaY, RenderLayer parentLayer) {
            sigmaX = BlurUtils.adjustSigma(sigmaX, out var scaleFactorX, out var radiusX);
            sigmaY = BlurUtils.adjustSigma(sigmaY, out var scaleFactorY, out var radiusY);

            var textureWidth = Mathf.CeilToInt((float) maskLayer.width / scaleFactorX);
            if (textureWidth < 1) {
                textureWidth = 1;
            }
            
            var textureHeight = Mathf.CeilToInt((float) maskLayer.height / scaleFactorY);
            if (textureHeight < 1) {
                textureHeight = 1;
            }

            var blurXLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskLayer.layerBounds,
                filterMode = FilterMode.Bilinear,
                noMSAA = true,
            };

            parentLayer.addLayer(blurXLayer);

            var blurYLayer = new RenderLayer {
                rtID = Shader.PropertyToID("_rtID_" + this._layers.Count + "_" + parentLayer.layers.Count),
                width = textureWidth,
                height = textureHeight,
                layerBounds = maskLayer.layerBounds,
                filterMode = FilterMode.Bilinear,
                noMSAA = true,
            };

            parentLayer.addLayer(blurYLayer);

            var blurMesh = ImageMeshGenerator.imageMesh(null, Rect.one, maskLayer.layerBounds);

            var kernelX = BlurUtils.get1DGaussianKernel(sigmaX, radiusX);
            var kernelY = BlurUtils.get1DGaussianKernel(sigmaY, radiusY);

            blurXLayer.draws.Add(CanvasShader.maskFilter(
                blurXLayer, blurMesh, maskLayer,
                radiusX, new Vector2(1f / textureWidth, 0), kernelX));

            blurYLayer.draws.Add(CanvasShader.maskFilter(
                blurYLayer, blurMesh, blurXLayer,
                radiusY, new Vector2(0, -1f / textureHeight), kernelY));

            return blurYLayer;
        }

        void _drawWithMaskFilter(Rect meshBounds, Action<Paint> drawAction, Paint paint, MaskFilter maskFilter) {
            var layer = this._currentLayer;
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

            var state = layer.currentState;
            float sigma = state.scale * maskFilter.sigma;
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

            var blurLayer = this._createBlurLayer(maskLayer, sigma, sigma, layer);

            var blurMesh = ImageMeshGenerator.imageMesh(null, Rect.one, maskBounds);
            if (!this._applyClip(blurMesh.bounds)) {
                return;
            }

            layer.draws.Add(CanvasShader.texRT(layer, paint, blurMesh, blurLayer));
        }

        void _drawPath(Path path, Paint paint) {
            D.assert(path != null);
            D.assert(paint != null);
            
            if (paint.style == PaintingStyle.fill) {
                var state = this._currentLayer.currentState;
                var cache = path.flatten(state.scale * this._devicePixelRatio);

                bool convex;
                var mesh = cache.getFillMesh(out convex).transform(state.matrix);
                
                Action<Paint> drawMesh = p => {
                    if (!this._applyClip(mesh.bounds)) {
                        return;
                    }

                    var layer = this._currentLayer;
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
                var state = this._currentLayer.currentState;
                float strokeWidth = (paint.strokeWidth * state.scale).clamp(0, 200.0f);
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
                    paint.strokeMiterLimit).transform(state.matrix);

                Action<Paint> drawMesh = p => {
                    if (!this._applyClip(mesh.bounds)) {
                        return;
                    }

                    var layer = this._currentLayer;
                    
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

        void _drawImage(Image image, Offset offset, Paint paint) {
            D.assert(image != null);
            D.assert(offset != null);
            D.assert(paint != null);

            this._drawImageRect(image,
                null,
                Rect.fromLTWH(
                    offset.dx, offset.dy,
                    image.width / this._devicePixelRatio,
                    image.height / this._devicePixelRatio),
                paint);
        }

        void _drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(dst != null);
            D.assert(paint != null);

            if (src == null) {
                src = Rect.one;
            } else {
                src = src.scale(1f / image.width, 1f / image.height);
            }

            var layer = this._currentLayer;
            var state = layer.currentState;
            var mesh = ImageMeshGenerator.imageMesh(state.matrix, src, dst);
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            layer.draws.Add(CanvasShader.tex(layer, paint, mesh, image));
        }

        void _drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
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

            var layer = this._currentLayer;
            var state = layer.currentState;

            var mesh = ImageMeshGenerator.imageNineMesh(state.matrix, src, center, image.width, image.height, dst);            
            if (!this._applyClip(mesh.bounds)) {
                return;
            }

            layer.draws.Add(CanvasShader.tex(layer, paint, mesh, image));
        }

        void _drawPicture(Picture picture, bool needsSave = true) {
            if (needsSave) {
                this._save();
            }

            int saveCount = 0;

            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                switch (drawCmd) {
                    case DrawSave _:
                        saveCount++;
                        this._save();
                        break;
                    case DrawSaveLayer cmd: {
                        saveCount++;
                        this._saveLayer(cmd.rect, cmd.paint);
                        break;
                    }
                    case DrawRestore _: {
                        saveCount--;
                        if (saveCount < 0) {
                            throw new Exception("unmatched save/restore in picture");
                        }

                        this._restore();
                        break;
                    }
                    case DrawTranslate cmd: {
                        this._translate(cmd.dx, cmd.dy);
                        break;
                    }
                    case DrawScale cmd: {
                        this._scale(cmd.sx, cmd.sy);
                        break;
                    }
                    case DrawRotate cmd: {
                        this._rotate(cmd.radians, cmd.offset);
                        break;
                    }
                    case DrawSkew cmd: {
                        this._skew(cmd.sx, cmd.sy);
                        break;
                    }
                    case DrawConcat cmd: {
                        this._concat(cmd.matrix);
                        break;
                    }
                    case DrawResetMatrix _:
                        this._resetMatrix();
                        break;
                    case DrawSetMatrix cmd: {
                        this._setMatrix(cmd.matrix);
                        break;
                    }
                    case DrawClipRect cmd: {
                        this._clipRect(cmd.rect);
                        break;
                    }
                    case DrawClipRRect cmd: {
                        this._clipRRect(cmd.rrect);
                        break;
                    }
                    case DrawClipPath cmd: {
                        this._clipPath(cmd.path);
                        break;
                    }
                    case DrawPath cmd: {
                        this._drawPath(cmd.path, cmd.paint);
                        break;
                    }
                    case DrawImage cmd: {
                        this._drawImage(cmd.image, cmd.offset, cmd.paint);
                        break;
                    }
                    case DrawImageRect cmd: {
                        this._drawImageRect(cmd.image, cmd.src, cmd.dst, cmd.paint);
                        break;
                    }
                    case DrawImageNine cmd: {
                        this._drawImageNine(cmd.image, cmd.src, cmd.center, cmd.dst, cmd.paint);
                        break;
                    }
                    case DrawPicture cmd: {
                        this._drawPicture(cmd.picture);
                        break;
                    }
                    case DrawTextBlob cmd: {
                        this._drawTextBlob(cmd.textBlob, cmd.offset, cmd.paint);
                        break;
                    }
                    default:
                        throw new Exception("unknown drawCmd: " + drawCmd);
                }
            }

            if (saveCount != 0) {
                throw new Exception("unmatched save/restore in picture");
            }

            if (needsSave) {
                this._restore();
            }
        }

        void _drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            D.assert(textBlob != null);
            D.assert(offset != null);
            D.assert(paint != null);

            var state = this._currentLayer.currentState;
            var scale = state.scale * this._devicePixelRatio;
            
            var matrix = new Matrix3(state.matrix);
            matrix.preTranslate(offset.dx, offset.dy);
            
            var mesh = new TextBlobMesh(textBlob, scale, matrix);
            var textBlobBounds = matrix.mapRect(textBlob.boundsInText);
            
            // request font texture so text mesh could be generated correctly
            var style = textBlob.style;
            var font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * scale);
            var subText = textBlob.text.Substring(textBlob.textOffset, textBlob.textSize);
            font.RequestCharactersInTextureSafe(subText, fontSizeToLoad, style.UnityFontStyle);

            var tex = font.material.mainTexture;

            Action<Paint> drawMesh = (Paint p) => {
                if (!this._applyClip(textBlobBounds)) {
                    return;
                }

                var layer = this._currentLayer;
                layer.draws.Add(CanvasShader.texAlpha(layer, p, mesh, tex));
            };

            if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                this._drawWithMaskFilter(textBlobBounds, drawMesh, paint, paint.maskFilter);
                return;
            }

            drawMesh(paint);
        }

        public void flush(Picture picture) {
            this._reset();

            this._drawPicture(picture, false);

            D.assert(this._layers.Count == 1);
            D.assert(this._layers[0].states.Count == 1);

            var layer = this._currentLayer;
            using (var cmdBuf = new CommandBuffer()) {
                cmdBuf.name = "CommandBufferCanvas";

                this._lastRtID = -1;
                this._drawLayer(layer, cmdBuf);

                // this is necessary for webgl2. not sure why... just to be safe to disable the scissor.
                cmdBuf.DisableScissorRect();

                Graphics.ExecuteCommandBuffer(cmdBuf);
            }

            this._clearLayer(layer);
        }

        int _lastRtID;

        void _setRenderTarget(CommandBuffer cmdBuf, int rtID, ref bool toClear) {
            if (this._lastRtID == rtID) {
                return;
            }

            this._lastRtID = rtID;
            
            if (rtID == 0) {
                cmdBuf.SetRenderTarget(this._renderTexture);
            } else {
                cmdBuf.SetRenderTarget(rtID);
            }
            
            if (toClear) {
                cmdBuf.ClearRenderTarget(true, true, UnityEngine.Color.clear);
                toClear = false;
            }
        }
        
        void _drawLayer(RenderLayer layer, CommandBuffer cmdBuf) {
            bool toClear = true;

            foreach (var cmdObj in layer.draws) {
                switch (cmdObj) {
                    case CmdLayer cmd:
                        var subLayer = cmd.layer;
                        var desc = new RenderTextureDescriptor(
                            subLayer.width, subLayer.height,
                            RenderTextureFormat.Default, 24) {
                            useMipMap = false,
                            autoGenerateMips = false,
                        };

                        if (this._renderTexture.antiAliasing != 0 && !subLayer.noMSAA) {
                            desc.msaaSamples = this._renderTexture.antiAliasing;
                        }

                        cmdBuf.GetTemporaryRT(subLayer.rtID, desc, subLayer.filterMode);
                        this._drawLayer(subLayer, cmdBuf);

                        break;
                    case CmdDraw cmd:
                        this._setRenderTarget(cmdBuf, layer.rtID, ref toClear);
                        
                        if (cmd.layer != null) {
                            if (cmd.layer.rtID == 0) {
                                cmdBuf.SetGlobalTexture(CmdDraw.texId, this._renderTexture);
                            } else {
                                cmdBuf.SetGlobalTexture(CmdDraw.texId, cmd.layer.rtID);
                            }
                        }

                        D.assert(cmd.meshObj == null);
                        cmd.meshObj = this._meshPool.getMesh();
                        cmd.meshObjCreated = true;

                        // clear triangles first in order to bypass validation in SetVertices.
                        cmd.meshObj.SetTriangles((int[]) null, 0, false);

                        MeshMesh mesh = cmd.mesh;
                        if (cmd.textMesh != null) {
                            mesh = cmd.textMesh.resovleMesh();
                        }

                        if (mesh == null) {
                            continue;
                        }

                        D.assert(mesh.vertices.Count > 0);
                        cmd.meshObj.SetVertices(mesh.vertices);
                        cmd.meshObj.SetTriangles(mesh.triangles, 0, false);
                        cmd.meshObj.SetUVs(0, mesh.uv);

                        if (mesh.matrix == null) {
                            cmd.properties.SetFloatArray(CmdDraw.matId, CmdDraw.idMat3.fMat);
                        } else {
                            cmd.properties.SetFloatArray(CmdDraw.matId, mesh.matrix.fMat);
                        }

                        cmdBuf.DrawMesh(cmd.meshObj, CmdDraw.idMat, cmd.material, 0, cmd.pass, cmd.properties);
                        if (cmd.layer != null) {
                            cmdBuf.SetGlobalTexture(CmdDraw.texId, BuiltinRenderTextureType.None);
                        }
                        break;
                    case CmdScissor cmd:
                        this._setRenderTarget(cmdBuf, layer.rtID, ref toClear);
                        
                        if (cmd.deviceScissor == null) {
                            cmdBuf.DisableScissorRect();
                        } else {
                            cmdBuf.EnableScissorRect(cmd.deviceScissor.toRect());
                        }
                        break;
                }
            }
            
            if (toClear) {
                this._setRenderTarget(cmdBuf, layer.rtID, ref toClear);
            }
            
            D.assert(!toClear);

            foreach (var subLayer in layer.layers) {
                cmdBuf.ReleaseTemporaryRT(subLayer.rtID);
            }
        }

        void _clearLayer(RenderLayer layer) {
            foreach (var cmdObj in layer.draws) {
                switch (cmdObj) {
                    case CmdDraw cmd:
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


        internal class RenderLayer {
            public int rtID;
            public int width;
            public int height;
            public FilterMode filterMode = FilterMode.Point;
            public bool noMSAA = false;
            public Rect layerBounds;
            public Paint layerPaint;
            public readonly List<object> draws = new List<object>();
            public readonly List<RenderLayer> layers = new List<RenderLayer>();
            public readonly List<State> states = new List<State>();
            public State currentState;
            public readonly ClipStack clipStack = new ClipStack();
            public uint lastClipGenId;
            public Rect lastClipBounds;
            public bool ignoreClip = true;

            Vector4? _viewport;

            public Vector4 viewport {
                get {
                    if (!this._viewport.HasValue) {
                        this._viewport = new Vector4(
                            this.layerBounds.left,
                            this.layerBounds.top,
                            this.layerBounds.width,
                            this.layerBounds.height);
                    }
                    return this._viewport.Value;
                }
            }

            public RenderLayer() {
                this.currentState = new State();
                this.states.Add(this.currentState);
            }

            public void addLayer(RenderLayer layer) {
                this.layers.Add(layer);
                this.draws.Add(new CmdLayer {layer = layer});
            }
        }

        internal class State {
            static readonly Matrix3 _id = Matrix3.I();
            
            Matrix3 _matrix;
            float? _scale;
            Matrix3 _invMatrix;

            public State(Matrix3 matrix = null, float? scale = null, Matrix3 invMatrix = null) {
                this._matrix = matrix ?? _id;
                this._scale = scale;
                this._invMatrix = invMatrix;
            }
            
            public Matrix3 matrix {
                get { return this._matrix; }
                set {
                    this._matrix = value ?? _id;
                    this._scale = null;
                    this._invMatrix = null;
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
            
            public Matrix3 invMatrix {
                get {
                    if (this._invMatrix == null) {
                        this._invMatrix = Matrix3.I();
                        this._matrix.invert(this._invMatrix);
                    }
                    return this._invMatrix;
                }
            }

            public State copy() {
                return new State(this._matrix, this._scale, this._invMatrix);
            }
        }
        
        
        internal class CmdLayer {
            public RenderLayer layer;
        }
        
        internal class CmdDraw {
            public MeshMesh mesh;
            public TextBlobMesh textMesh;
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

        internal class CmdScissor {
            public Rect deviceScissor;
        }
    }

    public class CommandBufferCanvas : RecorderCanvas {
        readonly PictureFlusher _flusher;
        
        public CommandBufferCanvas(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool) 
            : base(new PictureRecorder()) {
            this._flusher = new PictureFlusher(renderTexture, devicePixelRatio, meshPool);
        }

        public override float getDevicePixelRatio() {
            return this._flusher.getDevicePixelRatio();
        }

        public override void flush() {
            var picture = this._recorder.endRecording();
            this._recorder.reset();            
            this._flusher.flush(picture);
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

        public static MeshMesh imageMesh(Matrix3 matrix,
            Offset srcTL, Offset srcBL, Offset srcBR, Offset srcTR,
            Rect dst) {
            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            vertices.Add(new Vector2(dst.left, dst.top));
            uv.Add(new Vector2(srcTL.dx, 1.0f - srcTL.dy));
            vertices.Add(new Vector2(dst.left, dst.bottom));
            uv.Add(new Vector2(srcBL.dx, 1.0f - srcBL.dy));
            vertices.Add(new Vector2(dst.right, dst.bottom));
            uv.Add(new Vector2(srcBR.dx, 1.0f - srcBR.dy));
            vertices.Add(new Vector2(dst.right, dst.top));
            uv.Add(new Vector2(srcTR.dx, 1.0f - srcTR.dy));

            return new MeshMesh(matrix, vertices, _imageTriangles, uv);
        }
        
        public static MeshMesh imageMesh(Matrix3 matrix, Rect src, Rect dst) {
            var vertices = new List<Vector3>(4);
            var uv = new List<Vector2>(4);

            float uvx0 = src.left;
            float uvx1 = src.right;
            float uvy0 = 1.0f - src.top;
            float uvy1 = 1.0f - src.bottom;

            vertices.Add(new Vector2(dst.left, dst.top));
            uv.Add(new Vector2(uvx0, uvy0));
            vertices.Add(new Vector2(dst.left, dst.bottom));
            uv.Add(new Vector2(uvx0, uvy1));
            vertices.Add(new Vector2(dst.right, dst.bottom));
            uv.Add(new Vector2(uvx1, uvy1));
            vertices.Add(new Vector2(dst.right, dst.top));
            uv.Add(new Vector2(uvx1, uvy0));

            return new MeshMesh(matrix, vertices, _imageTriangles, uv);
        }

        public static MeshMesh imageNineMesh(Matrix3 matrix, Rect src, Rect center, int srcWidth, int srcHeight, Rect dst) {
            float x0 = dst.left;
            float x3 = dst.right;
            float x1 = x0 + ((center.left - src.left) * srcWidth);
            float x2 = x3 - ((src.right - center.right) * srcWidth);

            float y0 = dst.top;
            float y3 = dst.bottom;
            float y1 = y0 + ((center.top - src.top) * srcHeight);
            float y2 = y3 - ((src.bottom - center.bottom) * srcHeight);

            float tx0 = src.left;
            float tx1 = center.left;
            float tx2 = center.right;
            float tx3 = src.right;
            float ty0 = 1 - src.top;
            float ty1 = 1 - center.top;
            float ty2 = 1 - center.bottom;
            float ty3 = 1 - src.bottom;

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