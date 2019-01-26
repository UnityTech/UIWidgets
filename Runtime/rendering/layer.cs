using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public abstract class Layer : AbstractNodeMixinDiagnosticableTree {
        public new ContainerLayer parent {
            get { return (ContainerLayer) base.parent; }
        }

        public Layer nextSibling {
            get { return this._nextSibling; }
        }

        internal Layer _nextSibling;

        public Layer previousSibling {
            get { return this._previousSibling; }
        }

        internal Layer _previousSibling;

        public virtual void remove() {
            if (this.parent != null) {
                this.parent._removeChild(this);
            }
        }

        public void replaceWith(Layer newLayer) {
            D.assert(this.parent != null);
            D.assert(this.attached == this.parent.attached);
            D.assert(newLayer.parent == null);
            D.assert(newLayer._nextSibling == null);
            D.assert(newLayer._previousSibling == null);
            D.assert(!newLayer.attached);

            newLayer._nextSibling = this.nextSibling;
            if (this._nextSibling != null) {
                this._nextSibling._previousSibling = newLayer;
            }

            newLayer._previousSibling = this.previousSibling;
            if (this._previousSibling != null) {
                this._previousSibling._nextSibling = newLayer;
            }

            D.assert(() => {
                Layer node = this;
                while (node.parent != null) {
                    node = node.parent;
                }

                D.assert(node != newLayer);
                return true;
            });

            this.parent.adoptChild(newLayer);
            D.assert(newLayer.attached == this.parent.attached);

            if (this.parent.firstChild == this) {
                this.parent._firstChild = newLayer;
            }

            if (this.parent.lastChild == this) {
                this.parent._lastChild = newLayer;
            }

            this._nextSibling = null;
            this._previousSibling = null;
            this.parent.dropChild(this);
            D.assert(!this.attached);
        }

        public abstract void addToScene(SceneBuilder builder, Offset layerOffset);

        public object debugCreator;

        public override string toStringShort() {
            return base.toStringShort() + (this.owner == null ? "DETACHED" : "");
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("owner", this.owner,
                level: this.parent != null ? DiagnosticLevel.hidden : DiagnosticLevel.info,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<object>("creator", this.debugCreator,
                defaultValue: Diagnostics.kNullDefaultValue, level: DiagnosticLevel.debug));
        }
    }

    public class PictureLayer : Layer {
        public PictureLayer(Rect canvasBounds) {
            this.canvasBounds = canvasBounds;
        }

        public readonly Rect canvasBounds;

        public Picture picture;

        public bool isComplexHint = false;

        public bool willChangeHint = false;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            builder.addPicture(layerOffset, this.picture,
                isComplex: this.isComplexHint, willChange: this.willChangeHint);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Rect>("paint bounds", this.canvasBounds));
        }
    }

    public class ContainerLayer : Layer {
        public Layer firstChild {
            get { return this._firstChild; }
        }

        internal Layer _firstChild;

        public Layer lastChild {
            get { return this._lastChild; }
        }

        internal Layer _lastChild;

        bool _debugUltimatePreviousSiblingOf(Layer child, Layer equals = null) {
            D.assert(child.attached == this.attached);
            while (child.previousSibling != null) {
                D.assert(child.previousSibling != child);
                child = child.previousSibling;
                D.assert(child.attached == this.attached);
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(Layer child, Layer equals = null) {
            D.assert(child.attached == this.attached);
            while (child._nextSibling != null) {
                D.assert(child._nextSibling != child);
                child = child._nextSibling;
                D.assert(child.attached == this.attached);
            }

            return child == equals;
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
            D.assert(child != this);
            D.assert(child != this.firstChild);
            D.assert(child != this.lastChild);
            D.assert(child.parent == null);
            D.assert(!child.attached);
            D.assert(child.nextSibling == null);
            D.assert(child.previousSibling == null);
            D.assert(() => {
                Layer node = this;
                while (node.parent != null) {
                    node = node.parent;
                }

                D.assert(node != child);
                return true;
            });

            this.adoptChild(child);
            child._previousSibling = this.lastChild;
            if (this.lastChild != null) {
                this.lastChild._nextSibling = child;
            }

            this._lastChild = child;
            if (this._firstChild == null) {
                this._firstChild = child;
            }

            D.assert(child.attached == this.attached);
        }

        public void _removeChild(Layer child) {
            D.assert(child.parent == this);
            D.assert(child.attached == this.attached);
            D.assert(this._debugUltimatePreviousSiblingOf(child, equals: this.firstChild));
            D.assert(this._debugUltimateNextSiblingOf(child, equals: this.lastChild));

            if (child._previousSibling == null) {
                D.assert(this.firstChild == child);
                this._firstChild = child.nextSibling;
            }
            else {
                child._previousSibling._nextSibling = child.nextSibling;
            }

            if (child._nextSibling == null) {
                D.assert(this.lastChild == child);
                this._lastChild = child.previousSibling;
            }
            else {
                child._nextSibling._previousSibling = child.previousSibling;
            }

            D.assert((this.firstChild == null) == (this.lastChild == null));
            D.assert(this.firstChild == null || this.firstChild.attached == this.attached);
            D.assert(this.lastChild == null || this.lastChild.attached == this.attached);
            D.assert(this.firstChild == null ||
                     this._debugUltimateNextSiblingOf(this.firstChild, equals: this.lastChild));
            D.assert(this.lastChild == null ||
                     this._debugUltimatePreviousSiblingOf(this.lastChild, equals: this.firstChild));

            child._nextSibling = null;
            child._previousSibling = null;
            this.dropChild(child);
            D.assert(!child.attached);
        }

        public void removeAllChildren() {
            Layer child = this.firstChild;
            while (child != null) {
                Layer next = child.nextSibling;
                child._previousSibling = null;
                child._nextSibling = null;
                D.assert(child.attached == this.attached);
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

        public virtual void applyTransform(Layer child, Matrix3 transform) {
            D.assert(child != null);
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (this.firstChild == null) {
                return children;
            }

            Layer child = this.firstChild;
            int count = 1;
            while (true) {
                children.Add(child.toDiagnosticsNode(name: "child " + count));
                if (child == this.lastChild) {
                    break;
                }

                count += 1;
                child = child.nextSibling;
            }

            return children;
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

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("offset", this.offset));
        }
    }

    public class ClipRectLayer : ContainerLayer {
        public ClipRectLayer(
            Rect clipRect = null,
            Clip clipBehavior = Clip.hardEdge
        ) {
            D.assert(clipBehavior != Clip.none);
            this.clipRect = clipRect;
            this._clipBehavior = clipBehavior;
        }

        public readonly Rect clipRect;

        public Clip clipBehavior {
            get { return this._clipBehavior; }
            set {
                D.assert(value != Clip.none);
                this._clipBehavior = value;
            }
        }

        Clip _clipBehavior;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableClipLayers;
                return true;
            });

            if (enabled) {
                builder.pushClipRect(this.clipRect.shift(layerOffset));
            }

            this.addChildrenToScene(builder, layerOffset);

            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Rect>("clipRect", this.clipRect));
        }
    }

    public class ClipRRectLayer : ContainerLayer {
        public ClipRRectLayer(
            RRect clipRRect = null,
            Clip clipBehavior = Clip.hardEdge
        ) {
            D.assert(clipBehavior != Clip.none);
            this.clipRRect = clipRRect;
            this._clipBehavior = clipBehavior;
        }

        public readonly RRect clipRRect;

        public Clip clipBehavior {
            get { return this._clipBehavior; }
            set {
                D.assert(value != Clip.none);
                this._clipBehavior = value;
            }
        }

        Clip _clipBehavior;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableClipLayers;
                return true;
            });

            if (enabled) {
                builder.pushClipRRect(this.clipRRect.shift(layerOffset));
            }

            this.addChildrenToScene(builder, layerOffset);

            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<RRect>("clipRRect", this.clipRRect));
        }
    }

    public class TransformLayer : OffsetLayer {
        public TransformLayer(Matrix3 transform = null, Offset offset = null) : base(offset) {
            this._transform = transform ?? Matrix3.I();
        }

        public Matrix3 transform {
            get { return this._transform; }
            set { this._transform = value; }
        }

        Matrix3 _transform;
        Matrix3 _lastEffectiveTransform;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            this._lastEffectiveTransform = this._transform;

            var totalOffset = this.offset + layerOffset;
            if (totalOffset != Offset.zero) {
                this._lastEffectiveTransform = Matrix3.makeTrans((float) totalOffset.dx, (float) totalOffset.dy);
                this._lastEffectiveTransform.preConcat(this._transform);
            }

            builder.pushTransform(this._lastEffectiveTransform);
            this.addChildrenToScene(builder, Offset.zero);
            builder.pop();
        }

        public override void applyTransform(Layer child, Matrix3 transform) {
            D.assert(child != null);
            transform.preConcat(this._lastEffectiveTransform);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Matrix3>("transform", this.transform));
        }
    }

    public class OpacityLayer : ContainerLayer {
        public OpacityLayer(int alpha = 255) {
            this.alpha = alpha;
        }

        public int alpha;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableOpacityLayers;
                return true;
            });
            if (enabled) {
                builder.pushOpacity(this.alpha);
            }

            this.addChildrenToScene(builder, layerOffset);
            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("alpha", this.alpha));
        }
    }

    public class LayerLink {
        public LeaderLayer leader {
            get { return this._leader; }
        }

        internal LeaderLayer _leader;

        public override string ToString() {
            return $"{Diagnostics.describeIdentity(this)}({(this._leader != null ? "<linked>" : "<dangling>")})";
        }
    }

    public class LeaderLayer : ContainerLayer {
        public readonly LayerLink link;
        public Offset offset;

        public LeaderLayer(LayerLink link, Offset offset = null) {
            D.assert(link != null);
            offset = offset ?? Offset.zero;
            this.link = link;
            this.offset = offset;
        }

        public override void attach(object owner) {
            base.attach(owner);
            D.assert(this.link.leader == null);
            this._lastOffset = null;
            this.link._leader = this;
        }

        public override void detach() {
            D.assert(this.link.leader == this);
            this.link._leader = null;
            this._lastOffset = null;
            base.detach();
        }

        internal Offset _lastOffset;

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            D.assert(this.offset != null);
            this._lastOffset = this.offset + layerOffset;
            if (this._lastOffset != Offset.zero) {
                builder.pushTransform(Matrix3.makeTrans(this._lastOffset));
            }

            this.addChildrenToScene(builder, Offset.zero);
            if (this._lastOffset != Offset.zero) {
                builder.pop();
            }
        }

        public override void applyTransform(Layer child, Matrix3 transform) {
            D.assert(this._lastOffset != null);
            if (this._lastOffset != Offset.zero) {
                transform.preTranslate((float) this._lastOffset.dx, (float) this._lastOffset.dy);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("offset", this.offset));
            properties.add(new DiagnosticsProperty<LayerLink>("link", this.link));
        }
    }

    public class FollowerLayer : ContainerLayer {
        public FollowerLayer(
            LayerLink link = null,
            bool showWhenUnlinked = true,
            Offset unlinkedOffset = null,
            Offset linkedOffset = null
        ) {
            D.assert(link != null);
            this.link = link;
            this.showWhenUnlinked = showWhenUnlinked;
            this.unlinkedOffset = unlinkedOffset;
            this.linkedOffset = linkedOffset;
        }

        public readonly LayerLink link;
        public readonly bool showWhenUnlinked;
        public readonly Offset unlinkedOffset;
        public readonly Offset linkedOffset;

        Offset _lastOffset;
        Matrix3 _lastTransform;
        Matrix3 _invertedTransform;

        public Matrix3 getLastTransform() {
            if (this._lastTransform == null) {
                return null;
            }

            Matrix3 result = Matrix3.makeTrans((float) -this._lastOffset.dx, (float) -this._lastOffset.dy);
            result.preConcat(this._lastTransform);
            return result;
        }

        Matrix3 _collectTransformForLayerChain(List<ContainerLayer> layers) {
            Matrix3 result = Matrix3.I();
            for (int index = layers.Count - 1; index > 0; index -= 1) {
                layers[index].applyTransform(layers[index - 1], result);
            }

            return result;
        }

        void _establishTransform() {
            D.assert(this.link != null);
            this._lastTransform = null;
            if (this.link._leader == null) {
                return;
            }

            D.assert(this.link.leader.owner == this.owner,
                "Linked LeaderLayer anchor is not in the same layer tree as the FollowerLayer.");
            D.assert(this.link.leader._lastOffset != null,
                "LeaderLayer anchor must come before FollowerLayer in paint order, but the reverse was true.");

            HashSet<Layer> ancestors = new HashSet<Layer>();
            Layer ancestor = this.parent;
            while (ancestor != null) {
                ancestors.Add(ancestor);
                ancestor = ancestor.parent;
            }

            ContainerLayer layer = this.link.leader;
            List<ContainerLayer> forwardLayers = new List<ContainerLayer> {null, layer};
            do {
                layer = layer.parent;
                forwardLayers.Add(layer);
            } while (!ancestors.Contains(layer));

            ancestor = layer;

            layer = this;
            List<ContainerLayer> inverseLayers = new List<ContainerLayer>();
            do {
                layer = layer.parent;
                inverseLayers.Add(layer);
            } while (layer != ancestor);

            Matrix3 forwardTransform = this._collectTransformForLayerChain(forwardLayers);
            Matrix3 inverseTransform = this._collectTransformForLayerChain(inverseLayers);
            var inverse = Matrix3.I();
            var invertible = inverseTransform.invert(inverseTransform);
            if (!invertible) {
                return;
            }

            inverseTransform = inverse;
            inverseTransform.preConcat(forwardTransform);
            inverseTransform.preTranslate((float) this.linkedOffset.dx, (float) this.linkedOffset.dy);
            this._lastTransform = inverseTransform;
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset) {
            D.assert(this.link != null);
            if (this.link.leader == null && !this.showWhenUnlinked) {
                this._lastTransform = null;
                this._lastOffset = null;
                return;
            }

            this._establishTransform();
            if (this._lastTransform != null) {
                builder.pushTransform(this._lastTransform);
                this.addChildrenToScene(builder, Offset.zero);
                builder.pop();
                this._lastOffset = this.unlinkedOffset + layerOffset;
            }
            else {
                this._lastOffset = null;
                var matrix = Matrix3.makeTrans((float) this.unlinkedOffset.dx, (float) this.unlinkedOffset.dy);
                builder.pushTransform(matrix);
                this.addChildrenToScene(builder, Offset.zero);
                builder.pop();
            }
        }

        public override void applyTransform(Layer child, Matrix3 transform) {
            D.assert(child != null);
            D.assert(transform != null);
            if (this._lastTransform != null) {
                transform.preConcat(this._lastTransform);
            }
            else {
                transform.preConcat(Matrix3.makeTrans((float) this.unlinkedOffset.dx, (float) this.unlinkedOffset.dy));
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", this.link));
            properties.add(new TransformProperty("transform", this.getLastTransform(), defaultValue: null));
        }
    }
}