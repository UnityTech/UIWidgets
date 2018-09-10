using System;
using UIWidgets.gestures;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public class ViewConfiguration {
        public ViewConfiguration(
            Size size = null,
            double devicePixelRatio = 1.0
        ) {
            this.size = size ?? Size.zero;
            this.devicePixelRatio = devicePixelRatio;
        }

        public readonly Size size;

        public readonly double devicePixelRatio;

        public Matrix4x4 toMatrix() {
            return Matrix4x4.Scale(new Vector3(
                (float) this.devicePixelRatio, (float) this.devicePixelRatio, 1));
        }

        public override string ToString() {
            return string.Format("${0} at ${1}x", this.size, this.devicePixelRatio);
        }
    }

    public class RenderView : RenderObjectWithChildMixinRenderObject<RenderBox> {
        public RenderView(
            RenderBox child = null,
            ViewConfiguration configuration = null) {
            this.child = child;
            this._configuration = configuration;
        }

        public Size size {
            get { return this._size; }
        }

        public Size _size = Size.zero;

        public ViewConfiguration configuration {
            get { return this._configuration; }
            set {
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.replaceRootLayer((OffsetLayer) this._updateMatricesAndCreateNewRootLayer());
                this.markNeedsLayout();
            }
        }

        public ViewConfiguration _configuration;

        public void scheduleInitialFrame() {
            this.scheduleInitialLayout();
            this.scheduleInitialPaint((OffsetLayer) this._updateMatricesAndCreateNewRootLayer());
            this.owner.requestVisualUpdate();
        }

        public Matrix4x4 _rootTransform;

        public Layer _updateMatricesAndCreateNewRootLayer() {
            this._rootTransform = this.configuration.toMatrix();
            ContainerLayer rootLayer = new TransformLayer(transform: this._rootTransform);
            rootLayer.attach(this);
            return rootLayer;
        }

        public override void performResize() {
            throw new NotImplementedException();
        }

        public override void performLayout() {
            this._size = this.configuration.size;
            if (this.child != null) {
                this.child.layout(BoxConstraints.tight(this._size));
            }
        }

        public bool hitTest(HitTestResult result, Offset position = null) {
            if (this.child != null) {
                this.child.hitTest(result, position: position);
            }

            result.add(new HitTestEntry(this));
            return true;
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                context.paintChild(this.child, offset);
            }
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            transform *= this._rootTransform;
            base.applyPaintTransform(child, ref transform);
        }

        public void compositeFrame() {
            var builder = new SceneBuilder();
            this.layer.addToScene(builder, Offset.zero);
            var scene = builder.build();
            this.owner.binding.window.render(scene);
        }

        public override Rect paintBounds {
            get { return Offset.zero & (this.size * this.configuration.devicePixelRatio); }
        }
    }
}