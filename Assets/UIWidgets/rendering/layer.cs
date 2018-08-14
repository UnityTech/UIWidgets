using UIWidgets.foundation;
using UIWidgets.ui;

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

        public abstract S find<S>(Offset regionOffset);

        public abstract void addToScene(SceneBuilder builder, Offset layerOffset);
    }

    public class PictureLayer : Layer {
        public Picture picture;

        public override S find<S>(Offset regionOffset) {
            return default(S);
        }

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

        public override S find<S>(Offset regionOffset) {
            Layer current = this.lastChild;
            while (current != null) {
                var value = current.find<S>(regionOffset);
                if (value != null) {
                    return value;
                }

                current = current.previousSibling;
            }

            return default(S);
        }

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

        public virtual void applyTransform(Layer child, UnityEngine.Matrix4x4 transform) {
        }
    }

    public class OffsetLayer : ContainerLayer {
        public OffsetLayer(Offset offset = null) {
            this.offset = offset ?? Offset.zero;
        }

        public Offset offset;

        public override S find<S>(Offset regionOffset) {
            return base.find<S>(regionOffset - this.offset);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            this.addChildrenToScene(builder, this.offset + layerOffset);
        }
    }

    public class ClipRectLayer : ContainerLayer {
        public ClipRectLayer(Rect clipRect) {
            this.clipRect = clipRect;
        }

        public Rect clipRect;

        public override S find<S>(Offset regionOffset) {
            if (!this.clipRect.contains(regionOffset)) {
                return default(S);
            }

            return base.find<S>(regionOffset);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.pushClipRect(this.clipRect.shift(layerOffset));
            this.addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
    }

    public class TransformLayer : OffsetLayer {
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