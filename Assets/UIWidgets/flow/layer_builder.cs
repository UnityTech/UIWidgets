using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace UIWidgets.flow {
    public class LayerBuilder {
        private ContainerLayer _rootLayer;
        private ContainerLayer _currentLayer;

        private readonly Stack<Rect> _cullRects = new Stack<Rect>();

        public LayerBuilder() {
            this._cullRects.Push(Rect.largest);
        }

        private void pushLayer(ContainerLayer layer, Rect cullRect) {
            this._cullRects.Push(cullRect);

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

        public Layer takeLayer() {
            return this._rootLayer;
        }

        public void pop() {
            if (this._currentLayer == null) {
                return;
            }

            this._cullRects.Pop();
            this._currentLayer = this._currentLayer.parent;
        }

        public void pushTransform(Matrix4x4 matrix) {
            D.assert(matrix.determinant != 0.0);

            Rect cullRect = Rect.largest;
            if (!matrix.isPerspective()) {
                cullRect = matrix.inverse.transformRect(this._cullRects.Peek());
            }

            var layer = new TransformLayer();
            layer.transform = matrix;

            this.pushLayer(layer, cullRect);
        }

        public void pushClipRect(Rect clipRect) {
            Rect cullRect = clipRect.intersect(this._cullRects.Peek());

            var layer = new ClipRectLayer();
            layer.clipRect = clipRect;

            this.pushLayer(layer, cullRect);
        }

        public void pushClipRRect(RRect clipRRect) {
            Rect cullRect = clipRRect.outerRect.intersect(this._cullRects.Peek());

            var layer = new ClipRRectLayer();
            layer.clipRRect = clipRRect;

            this.pushLayer(layer, cullRect);
        }

        public void pushOpacity(int alpha) {
            var layer = new OpacityLayer();
            layer.alpha = alpha;

            this.pushLayer(layer, this._cullRects.Peek());
        }

        public void addPicture(Offset offset, Picture picture, bool isComplex, bool willChange) {
            if (this._currentLayer == null) {
                return;
            }

            Rect pictureRect = picture.paintBounds;
            pictureRect = pictureRect.shift(offset);

            if (!pictureRect.overlaps(this._cullRects.Peek())) {
                return;
            }

            var layer = new PictureLayer();
            layer.offset = offset;
            layer.picture = picture;
            layer.isComplex = isComplex;
            layer.willChange = willChange;
            this._currentLayer.add(layer);
        }
    }
}