using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.XR.WSA.Persistence;
using Canvas = UIWidgets.ui.Canvas;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public class ParentData {
        public virtual void detach() {
        }
    }

    public delegate void PaintingContextCallback(PaintingContext context, Offset offset);

    public class PaintingContext {
        private PaintingContext(ContainerLayer containerLayer) {
            this._containerLayer = containerLayer;
        }

        public readonly ContainerLayer _containerLayer;

        public static void repaintCompositedChild(RenderObject child) {
            if (child._layer == null) {
                child._layer = new OffsetLayer();
            } else {
                child._layer.removeAllChildren();
            }

            var childContext = new PaintingContext(child._layer);
            child._paintWithContext(childContext, Offset.zero);
            childContext._stopRecordingIfNeeded();
        }

        public void paintChild(RenderObject child, Offset offset) {
            if (child.isRepaintBoundary) {
                this._stopRecordingIfNeeded();
                this._compositeChild(child, offset);
            } else {
                child._paintWithContext(this, offset);
            }
        }

        public void _compositeChild(RenderObject child, Offset offset) {
            if (child._needsPaint) {
                PaintingContext.repaintCompositedChild(child);
            }

            child._layer.offset = offset;
            this._appendLayer(child._layer);
        }

        public void _appendLayer(Layer layer) {
            layer.remove();
            this._containerLayer.append(layer);
        }

        public bool _isRecording {
            get {
                bool hasCanvas = this._canvas != null;
                return hasCanvas;
            }
        }

        public PictureLayer _currentLayer;
        public PictureRecorder _recorder;
        public Canvas _canvas;

        public Canvas canvas {
            get {
                if (this._canvas == null) {
                    this._startRecording();
                }

                return this._canvas;
            }
        }

        public void _startRecording() {
            this._currentLayer = new PictureLayer();
            this._recorder = new PictureRecorder();
            this._canvas = new RecorderCanvas(this._recorder);
            this._containerLayer.append(this._currentLayer);
        }

        public void _stopRecordingIfNeeded() {
            if (!this._isRecording) {
                return;
            }

            this._currentLayer.picture = this._recorder.endRecording();
            this._currentLayer = null;
            this._recorder = null;
            this._canvas = null;
        }

        public void addLayer(Layer layer) {
            this._stopRecordingIfNeeded();
            this._appendLayer(layer);
        }

        void pushLayer(Layer childLayer, PaintingContextCallback painter, Offset offset) {
            this._stopRecordingIfNeeded();
            this._appendLayer(childLayer);
            var childContext = new PaintingContext((ContainerLayer) childLayer);
            painter(childContext, offset);
            childContext._stopRecordingIfNeeded();
        }

        public void pushClipRect(bool needsCompositing, Offset offset, Rect clipRect, PaintingContextCallback painter) {
            Rect offsetClipRect = clipRect.shift(offset);
            if (needsCompositing) {
                this.pushLayer(new ClipRectLayer(offsetClipRect), painter, offset);
            } else {
                this.canvas.save();
                this.canvas.clipRect(offsetClipRect);
                painter(this, offset);
                this.canvas.restore();
            }
        }

        public void pushClipRRect(bool needsCompositing, Offset offset, RRect clipRRect,
            PaintingContextCallback painter) {
            RRect offsetClipRRect = clipRRect.shift(offset);
            if (needsCompositing) {
                this.pushLayer(new ClipRRectLayer(offsetClipRRect), painter, offset);
            } else {
                this.canvas.save();
                this.canvas.clipRRect(offsetClipRRect);
                painter(this, offset);
                this.canvas.restore();
            }
        }

        void pushTransform(bool needsCompositing, Offset offset, Matrix4x4 transform, PaintingContextCallback painter) {
            var effectiveTransform = Matrix4x4.Translate(offset.toVector())
                                     * transform * Matrix4x4.Translate(-offset.toVector());

            if (needsCompositing) {
                this.pushLayer(new TransformLayer(effectiveTransform), painter, offset);
            } else {
                this.canvas.save();
                this.canvas.concat(effectiveTransform);
                painter(this, offset);
                this.canvas.restore();
            }
        }

        void pushOpacity(Offset offset, int alpha, PaintingContextCallback painter) {
            this.pushLayer(new OpacityLayer(alpha), painter, offset);
        }
    }

    public abstract class Constraints {
        public abstract bool isTight { get; }
        public abstract bool isNormalized { get; }
    }

    public delegate void RenderObjectVisitor(RenderObject child);

    public delegate void LayoutCallback<T>(T constraints) where T : Constraints;

    public class PipelineOwner {
        public PipelineOwner(
            RendererBinding binding = null,
            VoidCallback onNeedVisualUpdate = null) {
            this.binding = binding;
            this.onNeedVisualUpdate = onNeedVisualUpdate;
        }

        public readonly RendererBinding binding;

        public readonly VoidCallback onNeedVisualUpdate;

        public void requestVisualUpdate() {
            if (this.onNeedVisualUpdate != null) {
                this.onNeedVisualUpdate();
            }
        }

        public AbstractNode rootNode {
            get { return this._rootNode; }
            set {
                if (this._rootNode == value) {
                    return;
                }

                if (this._rootNode != null) {
                    this._rootNode.detach();
                }

                this._rootNode = value;
                if (this._rootNode != null) {
                    this._rootNode.attach(this);
                }
            }
        }

        public AbstractNode _rootNode;

        public List<RenderObject> _nodesNeedingLayout = new List<RenderObject>();

        public void flushLayout() {
            while (this._nodesNeedingLayout.Count > 0) {
                var dirtyNodes = this._nodesNeedingLayout;
                this._nodesNeedingLayout = new List<RenderObject>();
                dirtyNodes.Sort((a, b) => a.depth - b.depth);
                foreach (var node in dirtyNodes) {
                    if (node._needsLayout && node.owner == this) {
                        node._layoutWithoutResize();
                    }
                }
            }
        }

        public List<RenderObject> _nodesNeedingCompositingBitsUpdate = new List<RenderObject>();

        public void flushCompositingBits() {
            this._nodesNeedingCompositingBitsUpdate.Sort((a, b) => a.depth - b.depth);
            foreach (RenderObject node in this._nodesNeedingCompositingBitsUpdate) {
                if (node._needsCompositingBitsUpdate && node.owner == this) {
                    node._updateCompositingBits();
                }
            }

            this._nodesNeedingCompositingBitsUpdate.Clear();
        }

        public List<RenderObject> _nodesNeedingPaint = new List<RenderObject>();

        public void flushPaint() {
            var dirtyNodes = this._nodesNeedingPaint;
            this._nodesNeedingPaint = new List<RenderObject>();
            dirtyNodes.Sort((a, b) => a.depth - b.depth);
            foreach (var node in dirtyNodes) {
                if (node._needsPaint && node.owner == this) {
                    if (node._layer.attached) {
                        PaintingContext.repaintCompositedChild(node);
                    } else {
                        node._skippedPaintingOnLayer();
                    }
                }
            }
        }
    }

    public abstract class ContainerParentDataMixin<ChildType> : ParentData where ChildType : RenderObject {
        public ChildType previousSibling;

        public ChildType nextSibling;

        public override void detach() {
            base.detach();

            if (this.previousSibling != null) {
                var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
                previousSiblingParentData.nextSibling = this.nextSibling;
            }

            if (this.nextSibling != null) {
                var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
                nextSiblingParentData.previousSibling = this.previousSibling;
            }

            this.previousSibling = null;
            this.nextSibling = null;
        }
    }

    public abstract class RenderObject : AbstractNode {
        protected RenderObject() {
            this._needsCompositing = this.isRepaintBoundary || this.alwaysNeedsCompositing;
        }

        public ParentData parentData;

        public virtual void setupParentData(RenderObject child) {
            if (!(child.parentData is ParentData)) {
                child.parentData = new ParentData();
            }
        }

        public override void adoptChild(AbstractNode childNode) {
            var child = (RenderObject) childNode;
            this.setupParentData(child);
            base.adoptChild(child);
            this.markNeedsLayout();
            this.markNeedsCompositingBitsUpdate();
        }

        public override void dropChild(AbstractNode childNode) {
            var child = (RenderObject) childNode;
            child._cleanRelayoutBoundary();
            child.parentData.detach();
            child.parentData = null;
            base.dropChild(child);
            this.markNeedsLayout();
            this.markNeedsCompositingBitsUpdate();
        }

        public virtual void visitChildren(RenderObjectVisitor visitor) {
        }

        public new PipelineOwner owner {
            get { return (PipelineOwner) base.owner; }
        }

        public override void attach(object ownerObject) {
            var owner = (PipelineOwner) ownerObject;

            base.attach(owner);
            if (this._needsLayout && this._relayoutBoundary != null) {
                this._needsLayout = false;
                this.markNeedsLayout();
            }

            if (this._needsCompositingBitsUpdate) {
                this._needsCompositingBitsUpdate = false;
                this.markNeedsCompositingBitsUpdate();
            }

            if (this._needsPaint && this._layer != null) {
                this._needsPaint = false;
                this.markNeedsPaint();
            }
        }

        public bool _needsLayout = true;
        public RenderObject _relayoutBoundary;
        bool _doingThisLayoutWithCallback = false;

        public Constraints constraints {
            get { return this._constraints; }
        }

        public Constraints _constraints;

        public virtual void markNeedsLayout() {
            if (this._needsLayout) {
                return;
            }

            if (this._relayoutBoundary != this) {
                this.markParentNeedsLayout();
            } else {
                this._needsLayout = true;
                if (this.owner != null) {
                    this.owner._nodesNeedingLayout.Add(this);
                    this.owner.requestVisualUpdate();
                }
            }
        }

        public void markParentNeedsLayout() {
            this._needsLayout = true;
            if (!this._doingThisLayoutWithCallback) {
                ((RenderObject) this.parent).markNeedsLayout();
            }
        }

        public void markNeedsLayoutForSizedByParentChange() {
            this.markNeedsLayout();
            this.markParentNeedsLayout();
        }

        public void _cleanRelayoutBoundary() {
            if (this._relayoutBoundary != this) {
                this._relayoutBoundary = null;
                this._needsLayout = true;
                this.visitChildren(child => { child._cleanRelayoutBoundary(); });
            }
        }

        public void scheduleInitialLayout() {
            this._relayoutBoundary = this;
            this.owner._nodesNeedingLayout.Add(this);
        }

        public void _layoutWithoutResize() {
            try {
                this.performLayout();
            }
            catch (Exception ex) {
                Debug.LogError("error in performLayout: " + ex);
            }

            this._needsLayout = false;
            this.markNeedsPaint();
        }

        public void layout(Constraints constraints, bool parentUsesSize = false) {
            RenderObject relayoutBoundary;
            if (!parentUsesSize || this.sizedByParent || constraints.isTight || !(this.parent is RenderObject)) {
                relayoutBoundary = this;
            } else {
                relayoutBoundary = ((RenderObject) this.parent)._relayoutBoundary;
            }

            if (!this._needsLayout && object.Equals(constraints, this._constraints) &&
                relayoutBoundary == this._relayoutBoundary) {
                return;
            }

            this._constraints = constraints;
            this._relayoutBoundary = relayoutBoundary;
            if (this.sizedByParent) {
                try {
                    this.performResize();
                }
                catch (Exception ex) {
                    Debug.LogError("error in performResize: " + ex);
                }
            }

            try {
                this.performLayout();
            }
            catch (Exception ex) {
                Debug.LogError("error in performLayout: " + ex);
            }

            this._needsLayout = false;
            this.markNeedsPaint();
        }

        public bool sizedByParent {
            get { return false; }
        }

        public abstract void performResize();

        public abstract void performLayout();

        public void invokeLayoutCallback<T>(LayoutCallback<T> callback) where T : Constraints {
            this._doingThisLayoutWithCallback = true;
            try {
                callback((T) this.constraints);
            }
            finally {
                this._doingThisLayoutWithCallback = false;
            }
        }

        public virtual bool isRepaintBoundary {
            get { return false; }
        }

        public virtual bool alwaysNeedsCompositing {
            get { return false; }
        }

        public OffsetLayer _layer;

        public OffsetLayer layer {
            get { return this._layer; }
        }

        public bool _needsCompositingBitsUpdate = false;

        public void markNeedsCompositingBitsUpdate() {
            if (this._needsCompositingBitsUpdate) {
                return;
            }

            this._needsCompositingBitsUpdate = true;

            if (this.parent is RenderObject) {
                var parent = (RenderObject) this.parent;
                if (parent._needsCompositingBitsUpdate) {
                    return;
                }

                if (!this.isRepaintBoundary && !parent.isRepaintBoundary) {
                    parent.markNeedsCompositingBitsUpdate();
                    return;
                }
            }

            if (this.owner != null) {
                this.owner._nodesNeedingCompositingBitsUpdate.Add(this);
            }
        }

        public bool _needsCompositing;

        public bool needsCompositing {
            get { return _needsCompositing; }
        }

        public void _updateCompositingBits() {
            if (!this._needsCompositingBitsUpdate) {
                return;
            }

            bool oldNeedsCompositing = this._needsCompositing;
            this._needsCompositing = false;
            this.visitChildren(child => {
                child._updateCompositingBits();
                if (child.needsCompositing) {
                    this._needsCompositing = true;
                }
            });

            if (this.isRepaintBoundary || this.alwaysNeedsCompositing) {
                this._needsCompositing = true;
            }

            if (oldNeedsCompositing != this._needsCompositing) {
                this.markNeedsPaint();
            }

            this._needsCompositingBitsUpdate = false;
        }

        public bool _needsPaint = true;

        public void markNeedsPaint() {
            if (this._needsPaint) {
                return;
            }

            if (this.isRepaintBoundary) {
                if (this.owner != null) {
                    this.owner._nodesNeedingPaint.Add(this);
                    this.owner.requestVisualUpdate();
                }
            } else if (this.parent is RenderObject) {
                var parent = (RenderObject) this.parent;
                parent.markNeedsPaint();
            } else {
                if (this.owner != null) {
                    this.owner.requestVisualUpdate();
                }
            }
        }

        public void _skippedPaintingOnLayer() {
            AbstractNode ancestor = this.parent;
            while (ancestor is RenderObject) {
                var node = (RenderObject) ancestor;
                if (node.isRepaintBoundary) {
                    if (node._layer == null) {
                        break;
                    }

                    if (node._layer.attached) {
                        break;
                    }

                    node._needsPaint = true;
                }

                ancestor = ancestor.parent;
            }
        }

        public void scheduleInitialPaint(ContainerLayer rootLayer) {
            this._layer = (OffsetLayer) rootLayer;
            this.owner._nodesNeedingPaint.Add(this);
        }

        public void replaceRootLayer(OffsetLayer rootLayer) {
            this._layer.detach();
            this._layer = rootLayer;
            this.markNeedsPaint();
        }

        public void _paintWithContext(PaintingContext context, Offset offset) {
            if (this._needsLayout) {
                return;
            }

            this._needsPaint = false;
            try {
                this.paint(context, offset);
            }
            catch (Exception ex) {
                Debug.LogError("error in paint: " + ex);
            }
        }

        public abstract Rect paintBounds { get; }

        public virtual void paint(PaintingContext context, Offset offset) {
        }

        public virtual void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
        }

        public Matrix4x4 getTransformTo(RenderObject ancestor) {
            if (ancestor == null) {
                AbstractNode rootNode = this.owner.rootNode;
                if (rootNode is RenderObject) {
                    ancestor = (RenderObject) rootNode;
                }
            }

            var renderers = new List<RenderObject>();
            for (RenderObject renderer = this; renderer != ancestor; renderer = (RenderObject) renderer.parent) {
                renderers.Add(renderer);
            }

            var transform = Matrix4x4.identity;
            for (int index = renderers.Count - 1; index > 0; index -= 1) {
                renderers[index].applyPaintTransform(renderers[index - 1], ref transform);
            }

            return transform;
        }
    }
}