using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public abstract class Layer : AbstractNode {
        public new ContainerLayer parent {
            get { return (ContainerLayer) base.parent; }
        }

        public Layer nextSibling {
            get { return this._nextSibling; }
        }

        public Layer _nextSibling;


        public Layer previousSibling {
            get { return this._previousSibling; }
        }

        public Layer _previousSibling;

        public virtual void remove() {
            if (this.parent != null) {
                this.parent._removeChild(this);
            }
        }

        public void replaceWith(Layer newLayer) {
            newLayer._nextSibling = this.nextSibling;
            if (this._nextSibling != null) {
                this._nextSibling._previousSibling = newLayer;
            }

            newLayer._previousSibling = this.previousSibling;
            if (this._previousSibling != null) {
                this._previousSibling._nextSibling = newLayer;
            }

            this.parent.adoptChild(newLayer);
            if (this.parent.firstChild == this) {
                this.parent._firstChild = newLayer;
            }

            if (this.parent.lastChild == this) {
                this.parent._lastChild = newLayer;
            }

            this._nextSibling = null;
            this._previousSibling = null;
            this.parent.dropChild(this);
        }

        public abstract void addToScene(SceneBuilder builder, Offset layerOffset);
    }

    public class PictureLayer : Layer {
        public Picture picture;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.addPicture(layerOffset, this.picture);
        }
    }

    public class ContainerLayer : Layer {
        public Layer firstChild {
            get { return this._firstChild; }
        }

        public Layer _firstChild;

        public Layer lastChild {
            get { return this._lastChild; }
        }

        public Layer _lastChild;

        public override void attach(object owner) {
            base.attach(owner);

            var child = this.firstChild;
            while (child != null) {
                child.attach(owner);
                child = child.nextSibling;
            }
        }

        public override void detach() {
            base.detach();

            var child = this.firstChild;
            while (child != null) {
                child.detach();
                child = child.nextSibling;
            }
        }

        public void append(Layer child) {
            this.adoptChild(child);
            child._previousSibling = this.lastChild;
            if (this.lastChild != null) {
                this.lastChild._nextSibling = child;
            }

            this._lastChild = child;
            if (this._firstChild == null) {
                this._firstChild = child;
            }
        }

        public void _removeChild(Layer child) {
            if (child._previousSibling == null) {
                this._firstChild = child.nextSibling;
            } else {
                child._previousSibling._nextSibling = child.nextSibling;
            }

            if (child._nextSibling == null) {
                this._lastChild = child.previousSibling;
            } else {
                child._nextSibling._previousSibling = child.previousSibling;
            }

            child._nextSibling = null;
            child._previousSibling = null;
            this.dropChild(child);
        }

        public void removeAllChildren() {
            Layer child = this.firstChild;
            while (child != null) {
                Layer next = child.nextSibling;
                child._previousSibling = null;
                child._nextSibling = null;
                this.dropChild(child);
                child = next;
            }

            this._firstChild = null;
            this._lastChild = null;
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            this.addChildrenToScene(builder, layerOffset);
        }

        public void addChildrenToScene(SceneBuilder builder, Offset childOffset) {
            Layer child = this.firstChild;
            while (child != null) {
                child.addToScene(builder, childOffset);
                child = child.nextSibling;
            }
        }
    }

    public class OffsetLayer : ContainerLayer {
        public OffsetLayer(Offset offset = null) {
            this.offset = offset ?? Offset.zero;
        }

        public Offset offset;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            this.addChildrenToScene(builder, this.offset + layerOffset);
        }
    }

    public class ClipRectLayer : ContainerLayer {
        public ClipRectLayer(Rect clipRect) {
            this.clipRect = clipRect;
        }

        public Rect clipRect;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.pushClipRect(this.clipRect.shift(layerOffset));
            this.addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
    }

    public class ClipRRectLayer : ContainerLayer {
        public ClipRRectLayer(RRect clipRRect) {
            this.clipRRect = clipRRect;
        }

        public RRect clipRRect;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.pushClipRRect(this.clipRRect.shift(layerOffset));
            this.addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
    }

    public class TransformLayer : OffsetLayer {
        public TransformLayer(Matrix4x4 transform, Offset offset = null) : base(offset) {
            this._transform = transform;
        }

        public Matrix4x4 transform {
            get { return this._transform; }
            set { this._transform = value; }
        }

        public Matrix4x4 _transform;
        public Matrix4x4 _lastEffectiveTransform;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            this._lastEffectiveTransform = this.transform;

            var totalOffset = this.offset + layerOffset;
            if (totalOffset != Offset.zero) {
                this._lastEffectiveTransform =
                    Matrix4x4.Translate(totalOffset.toVector()) * this._lastEffectiveTransform;
            }

            builder.pushTransform(this._lastEffectiveTransform);
            this.addChildrenToScene(builder, Offset.zero);
            builder.pop();
        }
    }

    public class OpacityLayer : ContainerLayer {
        public OpacityLayer(int alpha) {
            this.alpha = alpha;
        }

        public int alpha;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.pushOpacity(this.alpha);
            this.addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
    }
}