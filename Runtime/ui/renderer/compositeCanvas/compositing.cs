using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class SceneBuilder {
        ContainerLayer _rootLayer;
        ContainerLayer _currentLayer;

        public SceneBuilder() {
        }

        public Scene build() {
            return new Scene(this._rootLayer);
        }

        void _pushLayer(ContainerLayer layer) {
            if (this._rootLayer == null) {
                this._rootLayer = layer;
                this._currentLayer = layer;
                return;
            }

            if (this._currentLayer == null) {
                return;
            }

            this._currentLayer.add(layer);
            this._currentLayer = layer;
        }

        public Layer pushTransform(Matrix3 matrix) {
            var layer = new TransformLayer();
            layer.transform = matrix;
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushOffset(float dx, float dy) {
            var layer = new TransformLayer();
            layer.transform = Matrix3.makeTrans(dx, dy);
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushClipRect(Rect clipRect) {
            var layer = new ClipRectLayer();
            layer.clipRect = clipRect;
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushClipRRect(RRect clipRRect) {
            var layer = new ClipRRectLayer();
            layer.clipRRect = clipRRect;
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushClipPath(Path clipPath) {
            var layer = new ClipPathLayer();
            layer.clipPath = clipPath;
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushOpacity(int alpha, Offset offset = null) {
            offset = offset ?? Offset.zero;

            var layer = new OpacityLayer();
            layer.alpha = alpha;
            layer.offset = offset;
            this._pushLayer(layer);
            return layer;
        }

        public Layer pushBackdropFilter(ImageFilter filter) {
            var layer = new BackdropFilterLayer();
            layer.filter = filter;
            this._pushLayer(layer);
            return layer;
        }


        public void addRetained(Layer layer) {
            if (this._currentLayer == null) {
                return;
            }

            this._currentLayer.add(layer);
        }

        public void pop() {
            if (this._currentLayer == null) {
                return;
            }

            this._currentLayer = this._currentLayer.parent;
        }

        public void addPicture(Offset offset, Picture picture,
            bool isComplexHint = false, bool willChangeHint = false) {
            D.assert(offset != null);
            D.assert(picture != null);

            if (this._currentLayer == null) {
                return;
            }

            var layer = new PictureLayer();
            layer.offset = offset;
            layer.picture = picture;
            layer.isComplex = isComplexHint;
            layer.willChange = willChangeHint;
            this._currentLayer.add(layer);
        }

        public void addTexture(
            Texture texture,
            Offset offset,
            float width,
            float height,
            bool freeze) {
            if (this._currentLayer == null) {
                return;
            }

            var layer = new TextureLayer();
            layer.offset = offset;
            layer.size = new Size(width, height);
            layer.texture = texture;
            layer.freeze = freeze;
            this._currentLayer.add(layer);
        }

        public void addPerformanceOverlay(int enabledOptions, Rect bounds) {
            if (this._currentLayer == null) {
                return;
            }

            var layer = new PerformanceOverlayLayer(enabledOptions);
            layer.paintBounds = Rect.fromLTRB(
                bounds.left,
                bounds.top,
                bounds.right,
                bounds.bottom
            );
            this._currentLayer.add(layer);
        }

        public Layer pushPhysicalShape(Path path, float elevation, Color color, Color shadowColor, Clip clipBehavior) {
            var layer = new PhysicalShapeLayer(clipBehavior);
            layer.path = path;
            layer.elevation = elevation;
            layer.color = color;
            layer.shadowColor = shadowColor;
            layer.devicePixelRatio = Window.instance.devicePixelRatio;

            this._pushLayer(layer);
            return layer;
        }
    }

    public class Scene : IDisposable {
        public Scene(Layer rootLayer) {
            this._layerTree = new LayerTree();
            this._layerTree.rootLayer = rootLayer;
        }

        readonly LayerTree _layerTree;

        public LayerTree takeLayerTree() {
            return this._layerTree;
        }

        public void Dispose() {
        }
    }
}