using System.Collections.Generic;
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
        }

        public void pushClipRect(Rect clipRect) {
            
        }
        
        public void pushOpacity(int alpha) {
        }

        public void pushPicture(Offset offset, Picture picture) {
            
        }
    }
}