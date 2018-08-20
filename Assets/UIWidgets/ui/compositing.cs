using System;
using UIWidgets.flow;
using UnityEngine;

namespace UIWidgets.ui {
    public class SceneBuilder {
        private readonly LayerBuilder _layerBuilder = new LayerBuilder();

        public void pushTransform(Matrix4x4 matrix) {
            this._layerBuilder.pushTransform(matrix);
        }

        public void pushClipRect(Rect rect) {
            this._layerBuilder.pushClipRect(rect);
        }

        public void pushOpacity(int alpha) {
            this._layerBuilder.pushOpacity(alpha);
        }

        public void pop() {
            this._layerBuilder.pop();
        }

        public void addPicture(Offset offset, Picture picture) {
            this._layerBuilder.addPicture(offset, picture);
        }

        public Scene build() {
            return new Scene(this._layerBuilder.takeLayer());
        }
    }

    public class Scene : IDisposable {
        public Scene(Layer rootLayer) {
            this._rootLayer = rootLayer;
        }

        private readonly Layer _rootLayer;

        public Layer takeLayer() {
            return this._rootLayer;
        }

        public void Dispose() {
        }
    }
}