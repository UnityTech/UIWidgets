using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        readonly RenderTexture _renderTexture;
        readonly float _fringeWidth;
        readonly float _devicePixelRatio;
        readonly MeshPool _meshPool;

        readonly List<RenderLayer> _layers = new List<RenderLayer>();
        RenderLayer _currentLayer;
        uiRect? _lastScissor;

        readonly List<string> _renderTextureKeys = new List<string>();
        int _curRenderTextureId = 0;

        string _getNewRenderTextureKey() {
            if (this._curRenderTextureId == this._renderTextureKeys.Count) {
                for (int i = 0; i < 32; i++) {
                    this._renderTextureKeys.Add($"_RtKey_{i + this._curRenderTextureId}");
                }
            }

            var ret = this._renderTextureKeys[this._curRenderTextureId];
            this._curRenderTextureId++;
            return ret;
        }

        void _resetRenderTextureId() {
            this._curRenderTextureId = 0;
        }

        public void dispose() {
            if (this._currentLayer != null) {
                this._clearLayer(this._currentLayer);
                ObjectPool<RenderLayer>.release(this._currentLayer);
                this._currentLayer = null;
                this._lastScissor = null;
                this._layers.Clear();
            }
            
            this._releaseComputeBuffer();
        }

        public PictureFlusher(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool) {
            D.assert(renderTexture);
            D.assert(devicePixelRatio > 0);
            D.assert(meshPool != null);

            this._renderTexture = renderTexture;
            this._fringeWidth = 1.0f / devicePixelRatio;
            this._devicePixelRatio = devicePixelRatio;
            this._meshPool = meshPool;

            this.___drawTextDrawMeshCallback = this._drawTextDrawMeshCallback;
            this.___drawPathDrawMeshCallback2 = this._drawPathDrawMeshCallback2;
            this.___drawPathDrawMeshCallback = this._drawPathDrawMeshCallback;
        }

        readonly _drawPathDrawMeshCallbackDelegate ___drawTextDrawMeshCallback;
        readonly _drawPathDrawMeshCallbackDelegate ___drawPathDrawMeshCallback2;
        readonly _drawPathDrawMeshCallbackDelegate ___drawPathDrawMeshCallback;

        public float getDevicePixelRatio() {
            return this._devicePixelRatio;
        }

        void _reset() {
            //clear all states
            D.assert(this._currentLayer == null && this._layers.Count == 0);
            var width = this._renderTexture.width;
            var height = this._renderTexture.height;

            var bounds = uiRectHelper.fromLTWH(0, 0,
                width * this._fringeWidth,
                height * this._fringeWidth);

            RenderLayer firstLayer = RenderLayer.create(
                width: width,
                height: height,
                layerBounds: bounds
            );

            this._layers.Add(firstLayer);
            this._currentLayer = firstLayer;
        }

        void _save() {
            var layer = this._currentLayer;
            layer.currentState = layer.currentState.copy();
            layer.states.Add(layer.currentState);
            layer.clipStack.save();
        }

        static uiOffset[] _cachedPoints = new uiOffset[4];

        void _saveLayer(uiRect bounds, uiPaint paint) {
            D.assert(bounds.width > 0);
            D.assert(bounds.height > 0);

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

            var layer = RenderLayer.create(
                rtID: Shader.PropertyToID(this._getNewRenderTextureKey()),
                width: textureWidth,
                height: textureHeight,
                layerBounds: bounds,
                layerPaint: paint
            );

            parentLayer.addLayer(layer);
            this._layers.Add(layer);
            this._currentLayer = layer;

            if (paint.backdrop != null) {
                
                if (paint.backdrop is _uiBlurImageFilter) {
                    var filter = (_uiBlurImageFilter) paint.backdrop;
                    if (!(filter.sigmaX == 0 && filter.sigmaY == 0)) {
                        _cachedPoints[0] = bounds.topLeft;
                        _cachedPoints[1] = bounds.bottomLeft;
                        _cachedPoints[2] = bounds.bottomRight;
                        _cachedPoints[3] = bounds.topRight;

                        state.matrix.Value.mapPoints(ref _cachedPoints);

                        var parentBounds = parentLayer.layerBounds;
                        for (int i = 0; i < 4; i++) {
                            _cachedPoints[i] = new uiOffset(
                                (_cachedPoints[i].dx - parentBounds.left) / parentBounds.width,
                                (_cachedPoints[i].dy - parentBounds.top) / parentBounds.height
                            );
                        }

                        var mesh = ImageMeshGenerator.imageMesh(
                            null,
                            _cachedPoints[0],
                            _cachedPoints[1],
                            _cachedPoints[2],
                            _cachedPoints[3],
                            bounds);
                        var renderDraw = CanvasShader.texRT(layer, layer.layerPaint.Value, mesh, parentLayer);
                        layer.draws.Add(renderDraw);

                        var blurLayer = this._createBlurLayer(layer, filter.sigmaX, filter.sigmaY, layer);
                        var blurMesh = ImageMeshGenerator.imageMesh(null, uiRectHelper.one, bounds);
                        layer.draws.Add(CanvasShader.texRT(layer, paint, blurMesh, blurLayer));
                    }
                }
                else if (paint.backdrop is _uiMatrixImageFilter) {
                    var filter = (_uiMatrixImageFilter) paint.backdrop;
                    if (!filter.transform.isIdentity()) {
                        layer.filterMode = filter.filterMode;

                        _cachedPoints[0] = bounds.topLeft;
                        _cachedPoints[1] = bounds.bottomLeft;
                        _cachedPoints[2] = bounds.bottomRight;
                        _cachedPoints[3] = bounds.topRight;
                        
                        state.matrix.Value.mapPoints(ref _cachedPoints);

                        var parentBounds = parentLayer.layerBounds;
                        for (int i = 0; i < 4; i++) {
                            _cachedPoints[i] = new uiOffset(
                                (_cachedPoints[i].dx - parentBounds.left) / parentBounds.width,
                                (_cachedPoints[i].dy - parentBounds.top) / parentBounds.height
                            );
                        }

                        var matrix = uiMatrix3.makeTrans(-bounds.left, -bounds.top);
                        matrix.postConcat(filter.transform);
                        matrix.postTranslate(bounds.left, bounds.top);

                        var mesh = ImageMeshGenerator.imageMesh(
                            matrix,
                            _cachedPoints[0],
                            _cachedPoints[1],
                            _cachedPoints[2],
                            _cachedPoints[3],
                            bounds);
                        var renderDraw = CanvasShader.texRT(layer, layer.layerPaint.Value, mesh, parentLayer);
                        layer.draws.Add(renderDraw);
                    }
                }
            }
        }

        void _restore() {
            var layer = this._currentLayer;
            D.assert(layer.states.Count > 0);
            if (layer.states.Count > 1) {
                ObjectPool<State>.release(layer.states[layer.states.Count - 1]);
                layer.states.RemoveAt(layer.states.Count - 1);
                layer.currentState = layer.states[layer.states.Count - 1];
                layer.clipStack.restore();
                return;
            }

            this._layers.RemoveAt(this._layers.Count - 1);
            var currentLayer = this._currentLayer = this._layers[this._layers.Count - 1];
            var state = currentLayer.currentState;

            var mesh = ImageMeshGenerator.imageMesh(state.matrix, uiRectHelper.one, layer.layerBounds);

            if (!this._applyClip(mesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(mesh);
                return;
            }

            var renderDraw = CanvasShader.texRT(currentLayer, layer.layerPaint.Value, mesh, layer);
            currentLayer.draws.Add(renderDraw);
        }

        void _translate(float dx, float dy) {
            var state = this._currentLayer.currentState;
            var matrix = uiMatrix3.makeTrans(dx, dy);
            matrix.postConcat(state.matrix.Value);
            state.matrix = matrix;
        }

        void _scale(float sx, float? sy = null) {
            var state = this._currentLayer.currentState;
            var matrix = uiMatrix3.makeScale(sx, (sy ?? sx));
            matrix.postConcat(state.matrix.Value);
            state.matrix = matrix;
        }

        void _rotate(float radians, uiOffset? offset = null) {
            var state = this._currentLayer.currentState;
            if (offset == null) {
                var matrix = uiMatrix3.makeRotate(radians);
                matrix.postConcat(state.matrix.Value);
                state.matrix = matrix;
            }
            else {
                var matrix = uiMatrix3.makeRotate(radians, offset.Value.dx, offset.Value.dy);
                matrix.postConcat(state.matrix.Value);
                state.matrix = matrix;
            }
        }

        void _skew(float sx, float sy) {
            var state = this._currentLayer.currentState;
            var matrix = uiMatrix3.makeSkew(sx, sy);
            matrix.postConcat(state.matrix.Value);
            state.matrix = matrix;
        }

        void _concat(uiMatrix3 matrix) {
            var state = this._currentLayer.currentState;
            matrix = new uiMatrix3(matrix);
            matrix.postConcat(state.matrix.Value);
            state.matrix = matrix;
        }

        void _resetMatrix() {
            var state = this._currentLayer.currentState;
            state.matrix = uiMatrix3.I();
        }

        void _setMatrix(uiMatrix3 matrix) {
            var state = this._currentLayer.currentState;
            state.matrix = new uiMatrix3(matrix);
        }

        void _clipRect(Rect rect) {
            var path = uiPath.create();
            path.addRect(uiRectHelper.fromRect(rect).Value);
            this._clipPath(path);
            uiPathCacheManager.putToCache(path);
        }

        void _clipUIRect(uiRect rect) {
            var path = uiPath.create();
            path.addRect(rect);
            this._clipPath(path);
            uiPathCacheManager.putToCache(path);
        }

        void _clipRRect(RRect rrect) {
            var path = uiPath.create();
            path.addRRect(rrect);
            this._clipPath(path);
            uiPathCacheManager.putToCache(path);
        }

        void _clipPath(uiPath path) {
            var layer = this._currentLayer;
            var state = layer.currentState;
            layer.clipStack.clipPath(path, state.matrix.Value, state.scale * this._devicePixelRatio);
        }

        void _tryAddScissor(RenderLayer layer, uiRect? scissor) {
            if (uiRectHelper.equals(scissor, this._lastScissor)) {
                return;
            }

            layer.draws.Add(CmdScissor.create(
                deviceScissor: scissor
            ));
            this._lastScissor = scissor;
        }

        bool _applyClip(uiRect? queryBounds) {
            if (queryBounds == null || queryBounds.Value.isEmpty) {
                return false;
            }

            var layer = this._currentLayer;
            var layerBounds = layer.layerBounds;
            ReducedClip reducedClip = ReducedClip.create(layer.clipStack, layerBounds, queryBounds.Value);
            if (reducedClip.isEmpty()) {
                ObjectPool<ReducedClip>.release(reducedClip);
                return false;
            }

            var scissor = reducedClip.scissor;
            var physicalRect = uiRectHelper.fromLTRB(0, 0, layer.width, layer.height);

            if (uiRectHelper.equals(scissor, layerBounds)) {
                this._tryAddScissor(layer, null);
            }
            else {
                var deviceScissor = uiRectHelper.fromLTRB(
                    scissor.Value.left - layerBounds.left, layerBounds.bottom - scissor.Value.bottom,
                    scissor.Value.right - layerBounds.left, layerBounds.bottom - scissor.Value.top
                );
                deviceScissor = uiRectHelper.scale(deviceScissor, layer.width / layerBounds.width,
                    layer.height / layerBounds.height);
                deviceScissor = uiRectHelper.roundOut(deviceScissor);
                deviceScissor = uiRectHelper.intersect(deviceScissor, physicalRect);

                if (deviceScissor.isEmpty) {
                    ObjectPool<ReducedClip>.release(reducedClip);
                    return false;
                }

                this._tryAddScissor(layer, deviceScissor);
            }

            var maskGenID = reducedClip.maskGenID();
            if (this._mustRenderClip(maskGenID, reducedClip.scissor.Value)) {
                if (maskGenID == ClipStack.wideOpenGenID) {
                    layer.ignoreClip = true;
                }
                else {
                    layer.ignoreClip = false;

                    // need to inflate a bit to make sure all area is cleared.
                    var inflatedScissor = uiRectHelper.inflate(reducedClip.scissor.Value, this._fringeWidth);
                    var boundsMesh = uiMeshMesh.create(inflatedScissor);
                    layer.draws.Add(CanvasShader.stencilClear(layer, boundsMesh));

                    foreach (var maskElement in reducedClip.maskElements) {
                        layer.draws.Add(CanvasShader.stencil0(layer, maskElement.mesh.duplicate()));
                        layer.draws.Add(CanvasShader.stencil1(layer, boundsMesh.duplicate()));
                    }
                }

                this._setLastClipGenId(maskGenID, reducedClip.scissor.Value);
            }

            ObjectPool<ReducedClip>.release(reducedClip);

            return true;
        }

        void _setLastClipGenId(uint clipGenId, uiRect clipBounds) {
            var layer = this._currentLayer;
            layer.lastClipGenId = clipGenId;
            layer.lastClipBounds = clipBounds;
        }

        bool _mustRenderClip(uint clipGenId, uiRect clipBounds) {
            var layer = this._currentLayer;
            return layer.lastClipGenId != clipGenId || !uiRectHelper.equals(layer.lastClipBounds, clipBounds);
        }

        RenderLayer _createMaskLayer(RenderLayer parentLayer, uiRect maskBounds,
            _drawPathDrawMeshCallbackDelegate drawCallback,
            uiPaint paint, bool convex, float alpha, float strokeMult, Texture tex, uiRect texBound,
            TextBlobMesh textMesh,
            uiMeshMesh fillMesh, uiMeshMesh strokeMesh, bool notEmoji) {
            var textureWidth = Mathf.CeilToInt(maskBounds.width * this._devicePixelRatio);
            if (textureWidth < 1) {
                textureWidth = 1;
            }

            var textureHeight = Mathf.CeilToInt(maskBounds.height * this._devicePixelRatio);
            if (textureHeight < 1) {
                textureHeight = 1;
            }

            var maskLayer = RenderLayer.create(
                rtID: Shader.PropertyToID(this._getNewRenderTextureKey()),
                width: textureWidth,
                height: textureHeight,
                layerBounds: maskBounds,
                filterMode: FilterMode.Bilinear,
                noMSAA: true
            );

            parentLayer.addLayer(maskLayer);
            this._layers.Add(maskLayer);
            this._currentLayer = maskLayer;

            var parentState = parentLayer.states[parentLayer.states.Count - 1];
            var maskState = maskLayer.states[maskLayer.states.Count - 1];
            maskState.matrix = parentState.matrix;

            drawCallback.Invoke(uiPaint.shapeOnly(paint), fillMesh, strokeMesh, convex, alpha, strokeMult, tex,
                texBound, textMesh, notEmoji);

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

            var blurXLayer = RenderLayer.create(
                rtID: Shader.PropertyToID(this._getNewRenderTextureKey()),
                width: textureWidth,
                height: textureHeight,
                layerBounds: maskLayer.layerBounds,
                filterMode: FilterMode.Bilinear,
                noMSAA: true
            );

            parentLayer.addLayer(blurXLayer);

            var blurYLayer = RenderLayer.create(
                rtID: Shader.PropertyToID(this._getNewRenderTextureKey()),
                width: textureWidth,
                height: textureHeight,
                layerBounds: maskLayer.layerBounds,
                filterMode: FilterMode.Bilinear,
                noMSAA: true
            );

            parentLayer.addLayer(blurYLayer);

            var blurMesh = ImageMeshGenerator.imageMesh(null, uiRectHelper.one, maskLayer.layerBounds);

            var kernelX = BlurUtils.get1DGaussianKernel(sigmaX, radiusX);
            var kernelY = BlurUtils.get1DGaussianKernel(sigmaY, radiusY);

            blurXLayer.draws.Add(CanvasShader.maskFilter(
                blurXLayer, blurMesh, maskLayer,
                radiusX, new Vector2(1f / textureWidth, 0), kernelX));

            blurYLayer.draws.Add(CanvasShader.maskFilter(
                blurYLayer, blurMesh.duplicate(), blurXLayer,
                radiusY, new Vector2(0, -1f / textureHeight), kernelY));

            return blurYLayer;
        }

        void _drawWithMaskFilter(uiRect meshBounds, uiPaint paint, uiMaskFilter maskFilter,
            uiMeshMesh fillMesh, uiMeshMesh strokeMesh, bool convex, float alpha, float strokeMult, Texture tex,
            uiRect texBound, TextBlobMesh textMesh, bool notEmoji,
            _drawPathDrawMeshCallbackDelegate drawCallback) {
            var layer = this._currentLayer;
            var clipBounds = layer.layerBounds;

            uiRect? stackBounds;
            bool iior;
            layer.clipStack.getBounds(out stackBounds, out iior);

            if (stackBounds != null) {
                clipBounds = uiRectHelper.intersect(clipBounds, stackBounds.Value);
            }

            if (clipBounds.isEmpty) {
                this._drawPathDrawMeshQuit(fillMesh, strokeMesh, textMesh);
                return;
            }

            var state = layer.currentState;
            float sigma = state.scale * maskFilter.sigma;
            if (sigma <= 0) {
                this._drawPathDrawMeshQuit(fillMesh, strokeMesh, textMesh);
                return;
            }

            float sigma3 = 3 * sigma;
            var maskBounds = uiRectHelper.inflate(meshBounds, sigma3);
            maskBounds = uiRectHelper.intersect(maskBounds, uiRectHelper.inflate(clipBounds, sigma3));
            if (maskBounds.isEmpty) {
                this._drawPathDrawMeshQuit(fillMesh, strokeMesh, textMesh);
                return;
            }

            var maskLayer = this._createMaskLayer(layer, maskBounds, drawCallback, paint, convex, alpha, strokeMult,
                tex, texBound,
                textMesh, fillMesh, strokeMesh, notEmoji);

            var blurLayer = this._createBlurLayer(maskLayer, sigma, sigma, layer);

            var blurMesh = ImageMeshGenerator.imageMesh(null, uiRectHelper.one, maskBounds);
            if (!this._applyClip(blurMesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(blurMesh);
                return;
            }

            layer.draws.Add(CanvasShader.texRT(layer, paint, blurMesh, blurLayer));
        }

        delegate void _drawPathDrawMeshCallbackDelegate(uiPaint p, uiMeshMesh fillMesh, uiMeshMesh strokeMesh,
            bool convex, float alpha, float strokeMult,
            Texture tex, uiRect textBlobBounds, TextBlobMesh textMesh, bool notEmoji);

        void _drawPathDrawMeshCallback(uiPaint p, uiMeshMesh fillMesh, uiMeshMesh strokeMesh, bool convex, float alpha,
            float strokeMult, Texture tex,
            uiRect textBlobBounds, TextBlobMesh textMesh, bool notEmoji) {
            if (!this._applyClip(fillMesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(fillMesh);
                ObjectPool<uiMeshMesh>.release(strokeMesh);
                return;
            }

            var layer = this._currentLayer;
            if (convex) {
                layer.draws.Add(CanvasShader.convexFill(layer, p, fillMesh));
            }
            else {
                layer.draws.Add(CanvasShader.fill0(layer, fillMesh));
                layer.draws.Add(CanvasShader.fill1(layer, p, fillMesh.boundsMesh));
            }

            if (strokeMesh != null) {
                layer.draws.Add(CanvasShader.strokeAlpha(layer, p, alpha, strokeMult, strokeMesh));
                layer.draws.Add(CanvasShader.stroke1(layer, strokeMesh.duplicate()));
            }
        }

        void _drawPathDrawMeshCallback2(uiPaint p, uiMeshMesh fillMesh, uiMeshMesh strokeMesh, bool convex, float alpha,
            float strokeMult, Texture tex,
            uiRect textBlobBounds, TextBlobMesh textMesh, bool notEmoji) {
            if (!this._applyClip(strokeMesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(strokeMesh);
                return;
            }

            var layer = this._currentLayer;

            layer.draws.Add(CanvasShader.strokeAlpha(layer, p, alpha, strokeMult, strokeMesh));
            layer.draws.Add(CanvasShader.stroke1(layer, strokeMesh.duplicate()));
        }

        void _drawTextDrawMeshCallback(uiPaint p, uiMeshMesh fillMesh, uiMeshMesh strokeMesh, bool convex, float alpha,
            float strokeMult, Texture tex,
            uiRect textBlobBounds, TextBlobMesh textMesh, bool notEmoji) {
            if (!this._applyClip(textBlobBounds)) {
                ObjectPool<TextBlobMesh>.release(textMesh);
                return;
            }

            var layer = this._currentLayer;
            if (notEmoji) {
                layer.draws.Add(CanvasShader.texAlpha(layer, p, textMesh, tex));
            }
            else {
                uiPaint paintWithWhite = new uiPaint(p);
                paintWithWhite.color = uiColor.white;
                if (EmojiUtils.image == null) {
                    ObjectPool<TextBlobMesh>.release(textMesh);
                    return;
                }

                var raw_mesh = textMesh.resolveMesh();
                var meshmesh = raw_mesh.duplicate();
                ObjectPool<TextBlobMesh>.release(textMesh);
                layer.draws.Add(CanvasShader.tex(layer, paintWithWhite, meshmesh, EmojiUtils.image));
            }
        }

        void _drawPathDrawMeshQuit(uiMeshMesh fillMesh, uiMeshMesh strokeMesh, TextBlobMesh textMesh) {
            ObjectPool<uiMeshMesh>.release(fillMesh);
            ObjectPool<uiMeshMesh>.release(strokeMesh);
            ObjectPool<TextBlobMesh>.release(textMesh);
        }

        void _drawPath(uiPath path, uiPaint paint) {
            D.assert(path != null);
            
            //draw fast shadow
            if (paint.maskFilter != null && paint.maskFilter.Value.style == BlurStyle.fast_shadow) {
                this._drawRRectShadow(path, paint);
                return;
            }

            if (paint.style == PaintingStyle.fill) {
                var state = this._currentLayer.currentState;
                var cache = path.flatten(state.scale * this._devicePixelRatio);

                bool convex;
                cache.computeFillMesh(this._fringeWidth, out convex);
                var fillMesh = cache.fillMesh;
                var strokeMesh = cache.strokeMesh;
                var fmesh = fillMesh.transform(state.matrix);
                var smesh = strokeMesh?.transform(state.matrix);
                float strokeMult = 1.0f;

                if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                    this._drawWithMaskFilter(fmesh.bounds, paint, paint.maskFilter.Value, fmesh, smesh, convex, 0,
                        strokeMult, null,
                        uiRectHelper.zero, null, false, this.___drawPathDrawMeshCallback);
                    return;
                }

                this._drawPathDrawMeshCallback(paint, fmesh, smesh, convex, 1.0f, strokeMult, null, uiRectHelper.zero,
                    null, false);
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

                strokeWidth = strokeWidth / state.scale * 0.5f;
                float strokeMult = (this._fringeWidth * 0.5f + strokeWidth * 0.5f) / this._fringeWidth;

                var cache = path.flatten(state.scale * this._devicePixelRatio);

                cache.computeStrokeMesh(
                    strokeWidth,
                    this._fringeWidth,
                    paint.strokeCap,
                    paint.strokeJoin,
                    paint.strokeMiterLimit);
                var strokeMesh = cache.strokeMesh;

                var mesh = strokeMesh.transform(state.matrix);

                if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                    this._drawWithMaskFilter(mesh.bounds, paint, paint.maskFilter.Value, null, mesh, false, alpha,
                        strokeMult, null,
                        uiRectHelper.zero, null, false, this.___drawPathDrawMeshCallback2);
                    return;
                }

                this._drawPathDrawMeshCallback2(paint, null, mesh, false, alpha, strokeMult, null, uiRectHelper.zero,
                    null, false);
            }
        }

        void _drawImage(Image image, uiOffset offset, uiPaint paint) {
            D.assert(image != null && image.valid);

            if (image == null || !image.valid) {
                return;
            }

            this._drawImageRect(image,
                null,
                uiRectHelper.fromLTWH(
                    offset.dx, offset.dy,
                    image.width / this._devicePixelRatio,
                    image.height / this._devicePixelRatio),
                paint);
        }

        void _drawImageRect(Image image, uiRect? src, uiRect dst, uiPaint paint) {
            D.assert(image != null && image.valid);

            if (image == null || !image.valid) {
                return;
            }

            if (src == null) {
                src = uiRectHelper.one;
            }
            else {
                src = uiRectHelper.scale(src.Value, 1f / image.width, 1f / image.height);
            }

            var layer = this._currentLayer;
            var state = layer.currentState;
            var mesh = ImageMeshGenerator.imageMesh(state.matrix, src.Value, dst);
            if (!this._applyClip(mesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(mesh);
                return;
            }

            layer.draws.Add(CanvasShader.tex(layer, paint, mesh, image));
        }

        void _drawImageNine(Image image, uiRect? src, uiRect center, uiRect dst, uiPaint paint) {
            D.assert(image != null && image.valid);

            if (image == null || !image.valid) {
                return;
            }

            var scaleX = 1f / image.width;
            var scaleY = 1f / image.height;
            if (src == null) {
                src = uiRectHelper.one;
            }
            else {
                src = uiRectHelper.scale(src.Value, scaleX, scaleY);
            }

            center = uiRectHelper.scale(center, scaleX, scaleY);

            var layer = this._currentLayer;
            var state = layer.currentState;

            var mesh = ImageMeshGenerator.imageNineMesh(state.matrix, src.Value, center, image.width, image.height,
                dst);
            if (!this._applyClip(mesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(mesh);
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
                        this._saveLayer(uiRectHelper.fromRect(cmd.rect).Value, uiPaint.fromPaint(cmd.paint));
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
                        this._rotate(cmd.radians, uiOffset.fromOffset(cmd.offset));
                        break;
                    }

                    case DrawSkew cmd: {
                        this._skew(cmd.sx, cmd.sy);
                        break;
                    }

                    case DrawConcat cmd: {
                        this._concat(uiMatrix3.fromMatrix3(cmd.matrix));
                        break;
                    }

                    case DrawResetMatrix _:
                        this._resetMatrix();
                        break;
                    case DrawSetMatrix cmd: {
                        this._setMatrix(uiMatrix3.fromMatrix3(cmd.matrix));
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
                        var uipath = uiPath.fromPath(cmd.path);
                        this._clipPath(uipath);
                        uiPathCacheManager.putToCache(uipath);
                        break;
                    }

                    case DrawPath cmd: {
                        var uipath = uiPath.fromPath(cmd.path);
                        this._drawPath(uipath, uiPaint.fromPaint(cmd.paint));
                        uiPathCacheManager.putToCache(uipath);
                        break;
                    }

                    case DrawImage cmd: {
                        this._drawImage(cmd.image, (uiOffset.fromOffset(cmd.offset)).Value,
                            uiPaint.fromPaint(cmd.paint));
                        break;
                    }

                    case DrawImageRect cmd: {
                        this._drawImageRect(cmd.image, uiRectHelper.fromRect(cmd.src), uiRectHelper.fromRect(cmd.dst).Value,
                            uiPaint.fromPaint(cmd.paint));
                        break;
                    }

                    case DrawImageNine cmd: {
                        this._drawImageNine(cmd.image, uiRectHelper.fromRect(cmd.src),
                            uiRectHelper.fromRect(cmd.center).Value, uiRectHelper.fromRect(cmd.dst).Value,
                            uiPaint.fromPaint(cmd.paint));
                        break;
                    }

                    case DrawPicture cmd: {
                        this._drawPicture(cmd.picture);
                        break;
                    }

                    case DrawTextBlob cmd: {
                        this._paintTextShadow(cmd.textBlob, cmd.offset);
                        this._drawTextBlob(cmd.textBlob, (uiOffset.fromOffset(cmd.offset)).Value,
                            uiPaint.fromPaint(cmd.paint));
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


        void _drawUIPicture(uiPicture picture, bool needsSave = true) {
            if (needsSave) {
                this._save();
            }

            int saveCount = 0;

            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                switch (drawCmd) {
                    case uiDrawSave _:
                        saveCount++;
                        this._save();
                        break;
                    case uiDrawSaveLayer cmd: {
                        saveCount++;
                        this._saveLayer(cmd.rect.Value, cmd.paint);
                        break;
                    }

                    case uiDrawRestore _: {
                        saveCount--;
                        if (saveCount < 0) {
                            throw new Exception("unmatched save/restore in picture");
                        }

                        this._restore();
                        break;
                    }

                    case uiDrawTranslate cmd: {
                        this._translate(cmd.dx, cmd.dy);
                        break;
                    }

                    case uiDrawScale cmd: {
                        this._scale(cmd.sx, cmd.sy);
                        break;
                    }

                    case uiDrawRotate cmd: {
                        this._rotate(cmd.radians, cmd.offset);
                        break;
                    }

                    case uiDrawSkew cmd: {
                        this._skew(cmd.sx, cmd.sy);
                        break;
                    }

                    case uiDrawConcat cmd: {
                        this._concat(cmd.matrix.Value);
                        break;
                    }

                    case uiDrawResetMatrix _:
                        this._resetMatrix();
                        break;
                    case uiDrawSetMatrix cmd: {
                        this._setMatrix(cmd.matrix.Value);
                        break;
                    }

                    case uiDrawClipRect cmd: {
                        this._clipUIRect(cmd.rect.Value);
                        break;
                    }

                    case uiDrawClipRRect cmd: {
                        this._clipRRect(cmd.rrect);
                        break;
                    }

                    case uiDrawClipPath cmd: {
                        this._clipPath(cmd.path);
                        break;
                    }

                    case uiDrawPath cmd: {
                        this._drawPath(cmd.path, cmd.paint);
                        break;
                    }

                    case uiDrawImage cmd: {
                        this._drawImage(cmd.image, cmd.offset.Value, cmd.paint);
                        break;
                    }

                    case uiDrawImageRect cmd: {
                        this._drawImageRect(cmd.image, cmd.src, cmd.dst.Value, cmd.paint);
                        break;
                    }

                    case uiDrawImageNine cmd: {
                        this._drawImageNine(cmd.image, cmd.src, cmd.center.Value, cmd.dst.Value, cmd.paint);
                        break;
                    }

                    case uiDrawPicture cmd: {
                        this._drawPicture(cmd.picture);
                        break;
                    }

                    case uiDrawTextBlob cmd: {
                        this._paintTextShadow(cmd.textBlob, new Offset(cmd.offset.Value.dx, cmd.offset.Value.dy));
                        this._drawTextBlob(cmd.textBlob, cmd.offset.Value, cmd.paint);
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

        void _paintTextShadow(TextBlob? textBlob, Offset offset) {
            D.assert(textBlob != null);
            if (textBlob.Value.style.shadows != null && textBlob.Value.style.shadows.isNotEmpty()) {
                textBlob.Value.style.shadows.ForEach(shadow => {
                    Paint paint = new Paint {
                        color = shadow.color,
                        maskFilter = shadow.blurRadius != 0
                            ? MaskFilter.blur(BlurStyle.normal, shadow.blurRadius)
                            : null,
                    };
                    this._drawTextBlob(textBlob, uiOffset.fromOffset(shadow.offset + offset).Value,
                        uiPaint.fromPaint(paint));
                });
            }
        }

        void _drawTextBlob(TextBlob? textBlob, uiOffset offset, uiPaint paint) {
            D.assert(textBlob != null);

            var state = this._currentLayer.currentState;
            var scale = state.scale * this._devicePixelRatio;

            var matrix = new uiMatrix3(state.matrix.Value);
            matrix.preTranslate(offset.dx, offset.dy);

            var mesh = TextBlobMesh.create(textBlob.Value, scale, matrix);
            var textBlobBounds = matrix.mapRect(uiRectHelper.fromRect(textBlob.Value.boundsInText).Value);

            // request font texture so text mesh could be generated correctly
            var style = textBlob.Value.style;
            var font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * scale);
            var subText = textBlob.Value.text.Substring(textBlob.Value.textOffset, textBlob.Value.textSize);
            Texture tex = null;
            bool notEmoji = !char.IsHighSurrogate(subText[0]) && !EmojiUtils.isSingleCharEmoji(subText[0]);
            if (notEmoji) {
                font.RequestCharactersInTextureSafe(subText, fontSizeToLoad, style.UnityFontStyle);
                tex = font.material.mainTexture;
            }

            if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                this._drawWithMaskFilter(textBlobBounds, paint, paint.maskFilter.Value, null, null, false, 0, 0,
                    notEmoji ? tex : EmojiUtils.image.texture,
                    textBlobBounds, mesh, true, this.___drawTextDrawMeshCallback);
                return;
            }

            this._drawTextDrawMeshCallback(paint, null, null, false, 0, 0, tex, textBlobBounds, mesh, notEmoji);
            
        }

        public void flush(uiPicture picture) {
            this._reset();
            this._resetRenderTextureId();
            this._resetComputeBuffer();

            this._drawUIPicture(picture, false);

            D.assert(this._layers.Count == 1);
            D.assert(this._layers[0].states.Count == 1);

            var layer = this._currentLayer;
            using (var cmdBuf = new CommandBuffer()) {
                cmdBuf.name = "CommandBufferCanvas";

                this._lastRtID = -1;
                this._drawLayer(layer, cmdBuf);

                // this is necessary for webgl2. not sure why... just to be safe to disable the scissor.
                cmdBuf.DisableScissorRect();

                this._bindComputeBuffer();
                Graphics.ExecuteCommandBuffer(cmdBuf);
            }

            D.assert(this._layers.Count == 0 || (this._layers.Count == 1 && this._layers[0] == this._currentLayer));
            if (this._currentLayer != null) {
                this._clearLayer(this._currentLayer);
                ObjectPool<RenderLayer>.release(this._currentLayer);
                this._currentLayer = null;
                this._lastScissor = null;
                this._layers.Clear();
            }

            AllocDebugger.onFrameEnd();
        }

        int _lastRtID;

        void _setRenderTarget(CommandBuffer cmdBuf, int rtID, ref bool toClear) {
            if (this._lastRtID == rtID) {
                return;
            }

            this._lastRtID = rtID;

            if (rtID == 0) {
                cmdBuf.SetRenderTarget(this._renderTexture);
            }
            else {
                cmdBuf.SetRenderTarget(rtID);
            }

            if (toClear) {
                cmdBuf.ClearRenderTarget(true, true, UnityEngine.Color.clear);
                toClear = false;
            }
        }

        readonly float[] _drawLayer_matArray = new float[9];

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
                            autoGenerateMips = false
                        };
                        
                        if (this._renderTexture.antiAliasing != 0 && !subLayer.noMSAA) {
                            desc.msaaSamples = this._renderTexture.antiAliasing;
                        }

                        cmdBuf.GetTemporaryRT(subLayer.rtID, desc, subLayer.filterMode);
                        this._drawLayer(subLayer, cmdBuf);

                        break;
                    case CmdDraw cmd:
                        this._setRenderTarget(cmdBuf, layer.rtID, ref toClear);

                        if (cmd.layerId != null) {
                            if (cmd.layerId == 0) {
                                cmdBuf.SetGlobalTexture(CmdDraw.texId, this._renderTexture);
                            }
                            else {
                                cmdBuf.SetGlobalTexture(CmdDraw.texId, cmd.layerId.Value);
                            }
                        }

                        D.assert(cmd.meshObj == null);
                        cmd.meshObj = this._meshPool.getMesh();
                        cmd.meshObjCreated = true;

                        // clear triangles first in order to bypass validation in SetVertices.
                        cmd.meshObj.SetTriangles((int[]) null, 0, false);

                        uiMeshMesh mesh = cmd.mesh;
                        if (cmd.textMesh != null) {
                            mesh = cmd.textMesh.resolveMesh();
                        }

                        if (mesh == null) {
                            continue;
                        }
                        
                        if (mesh.matrix == null) {
                            cmd.properties.SetFloatArray(CmdDraw.matId, CmdDraw.idMat3.fMat);
                        }
                        else {
                            var mat = mesh.matrix.Value;

                            this._drawLayer_matArray[0] = mat.kMScaleX;
                            this._drawLayer_matArray[1] = mat.kMSkewX;
                            this._drawLayer_matArray[2] = mat.kMTransX;
                            this._drawLayer_matArray[3] = mat.kMSkewY;
                            this._drawLayer_matArray[4] = mat.kMScaleY;
                            this._drawLayer_matArray[5] = mat.kMTransY;
                            this._drawLayer_matArray[6] = mat.kMPersp0;
                            this._drawLayer_matArray[7] = mat.kMPersp1;
                            this._drawLayer_matArray[8] = mat.kMPersp2;
                            cmd.properties.SetFloatArray(CmdDraw.matId, this._drawLayer_matArray);
                        }

                        D.assert(mesh.vertices.Count > 0);
                        if (this.supportComputeBuffer) {
                            this._addMeshToComputeBuffer(mesh.vertices?.data, mesh.uv?.data, mesh.triangles?.data);
                            cmd.properties.SetBuffer(CmdDraw.vertexBufferId, this._computeBuffer);
                            cmd.properties.SetBuffer(CmdDraw.indexBufferId, this._indexBuffer);
                            cmd.properties.SetInt(CmdDraw.startIndexId, this._startIndex);
                            cmdBuf.DrawProcedural(Matrix4x4.identity, cmd.material, cmd.pass, MeshTopology.Triangles, mesh.triangles.Count, 1, cmd.properties.mpb);
                        }
                        else {
                            cmd.meshObj.SetVertices(mesh.vertices?.data);
                            cmd.meshObj.SetTriangles(mesh.triangles?.data, 0, false);
                            cmd.meshObj.SetUVs(0, mesh.uv?.data);

                            cmdBuf.DrawMesh(cmd.meshObj, CmdDraw.idMat, cmd.material, 0, cmd.pass, cmd.properties.mpb);
                        }

                        if (cmd.layerId != null) {
                            cmdBuf.SetGlobalTexture(CmdDraw.texId, BuiltinRenderTextureType.None);
                        }

                        break;
                    case CmdScissor cmd:
                        this._setRenderTarget(cmdBuf, layer.rtID, ref toClear);

                        if (cmd.deviceScissor == null) {
                            cmdBuf.DisableScissorRect();
                        }
                        else {
                            cmdBuf.EnableScissorRect(uiRectHelper.toRect(cmd.deviceScissor.Value));
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

                cmdObj.release();
            }

            layer.draws.Clear();

            foreach (var subLayer in layer.layers) {
                this._clearLayer(subLayer);
                ObjectPool<RenderLayer>.release(subLayer);
            }

            layer.layers.Clear();
        }
    }
}