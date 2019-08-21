using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class ParentData {
        public virtual void detach() {
        }

        public override string ToString() {
            return "<none>";
        }
    }

    public delegate void PaintingContextCallback(PaintingContext context, Offset offset);

    public class PaintingContext : ClipContext {
        PaintingContext(
            ContainerLayer containerLayer = null,
            Rect estimatedBounds = null
        ) {
            D.assert(containerLayer != null);
            D.assert(estimatedBounds != null);
            this._containerLayer = containerLayer;
            this.estimatedBounds = estimatedBounds;
        }

        readonly ContainerLayer _containerLayer;

        public readonly Rect estimatedBounds;

        public static void repaintCompositedChild(RenderObject child, bool debugAlsoPaintedParent = false) {
            D.assert(child._needsPaint);

            _repaintCompositedChild(
                child,
                debugAlsoPaintedParent: debugAlsoPaintedParent
            );
        }

        static void _repaintCompositedChild(
            RenderObject child,
            bool debugAlsoPaintedParent = false,
            PaintingContext childContext = null
        ) {
            D.assert(child.isRepaintBoundary);
            D.assert(() => {
                child.debugRegisterRepaintBoundaryPaint(
                    includedParent: debugAlsoPaintedParent,
                    includedChild: true
                );
                return true;
            });
            if (child._layer == null) {
                D.assert(debugAlsoPaintedParent);
                child._layer = new OffsetLayer();
            }
            else {
                D.assert(debugAlsoPaintedParent || child._layer.attached);
                child._layer.removeAllChildren();
            }

            D.assert(() => {
                child._layer.debugCreator = child.debugCreator ?? child.GetType().ToString();
                return true;
            });
            childContext = childContext ?? new PaintingContext(child._layer, child.paintBounds);
            child._paintWithContext(childContext, Offset.zero);
            childContext.stopRecordingIfNeeded();
        }

        public static void debugInstrumentRepaintCompositedChild(
            RenderObject child,
            bool debugAlsoPaintedParent = false,
            PaintingContext customContext = null
        ) {
            D.assert(() => {
                _repaintCompositedChild(
                    child,
                    debugAlsoPaintedParent: debugAlsoPaintedParent,
                    childContext: customContext
                );
                return true;
            });
        }

        public void paintChild(RenderObject child, Offset offset) {
            if (child.isRepaintBoundary) {
                this.stopRecordingIfNeeded();
                this._compositeChild(child, offset);
            }
            else {
                child._paintWithContext(this, offset);
            }
        }

        void _compositeChild(RenderObject child, Offset offset) {
            D.assert(!this._isRecording);
            D.assert(child.isRepaintBoundary);
            D.assert(this._canvas == null || this._canvas.getSaveCount() == 1);

            if (child._needsPaint) {
                repaintCompositedChild(child, debugAlsoPaintedParent: true);
            }
            else {
                D.assert(child._layer != null);
                D.assert(() => {
                    child.debugRegisterRepaintBoundaryPaint(
                        includedParent: true,
                        includedChild: false
                    );
                    child._layer.debugCreator = child.debugCreator ?? child;
                    return true;
                });
            }

            child._layer.offset = offset;
            this.appendLayer(child._layer);
        }

        protected virtual void appendLayer(Layer layer) {
            D.assert(!this._isRecording);

            layer.remove();
            this._containerLayer.append(layer);
        }

        bool _isRecording {
            get {
                bool hasCanvas = this._canvas != null;
                D.assert(() => {
                    if (hasCanvas) {
                        D.assert(this._currentLayer != null);
                        D.assert(this._recorder != null);
                        D.assert(this._canvas != null);
                    }
                    else {
                        D.assert(this._currentLayer == null);
                        D.assert(this._recorder == null);
                        D.assert(this._canvas == null);
                    }

                    return true;
                });

                return hasCanvas;
            }
        }

        PictureLayer _currentLayer;
        PictureRecorder _recorder;
        Canvas _canvas;

        public override Canvas canvas {
            get {
                if (this._canvas == null) {
                    this._startRecording();
                }

                return this._canvas;
            }
        }

        void _startRecording() {
            D.assert(!this._isRecording);

            this._currentLayer = new PictureLayer(this.estimatedBounds);
            this._recorder = new PictureRecorder();
            this._canvas = new RecorderCanvas(this._recorder);
            this._containerLayer.append(this._currentLayer);
        }

        protected virtual void stopRecordingIfNeeded() {
            if (!this._isRecording) {
                return;
            }

            D.assert(() => {
                if (D.debugRepaintRainbowEnabled) {
                    var paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 6.0f,
                        color = D.debugCurrentRepaintColor.toColor()
                    };
                    this.canvas.drawRect(this.estimatedBounds.deflate(3.0f), paint);
                }

                if (D.debugPaintLayerBordersEnabled) {
                    Paint paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 1.0f,
                        color = new Color(0xFFFF9800),
                    };
                    this.canvas.drawRect(this.estimatedBounds, paint);
                }

                return true;
            });

            this._currentLayer.picture = this._recorder.endRecording();
            this._currentLayer = null;
            this._recorder = null;
            this._canvas = null;
        }

        public void setIsComplexHint() {
            if (this._currentLayer != null) {
                this._currentLayer.isComplexHint = true;
            }
        }

        public void setWillChangeHint() {
            if (this._currentLayer != null) {
                this._currentLayer.willChangeHint = true;
            }
        }

        public void addLayer(Layer layer) {
            this.stopRecordingIfNeeded();
            this.appendLayer(layer);
        }

        public void pushLayer(ContainerLayer childLayer, PaintingContextCallback painter, Offset offset,
            Rect childPaintBounds = null) {
            D.assert(!childLayer.attached);
            D.assert(childLayer.parent == null);
            D.assert(painter != null);

            this.stopRecordingIfNeeded();
            this.appendLayer(childLayer);

            var childContext = this.createChildContext(childLayer, childPaintBounds ?? this.estimatedBounds);
            painter(childContext, offset);
            childContext.stopRecordingIfNeeded();
        }

        protected PaintingContext createChildContext(ContainerLayer childLayer, Rect bounds) {
            return new PaintingContext(childLayer, bounds);
        }

        public void pushClipRect(bool needsCompositing, Offset offset, Rect clipRect, PaintingContextCallback painter,
            Clip clipBehavior = Clip.hardEdge) {
            Rect offsetClipRect = clipRect.shift(offset);
            if (needsCompositing) {
                this.pushLayer(new ClipRectLayer(offsetClipRect, clipBehavior: clipBehavior),
                    painter, offset, childPaintBounds: offsetClipRect);
            }
            else {
                this.clipRectAndPaint(offsetClipRect, clipBehavior, offsetClipRect, () => painter(this, offset));
            }
        }

        public void pushClipRRect(bool needsCompositing, Offset offset, Rect bounds, RRect clipRRect,
            PaintingContextCallback painter, Clip clipBehavior = Clip.antiAlias) {
            Rect offsetBounds = bounds.shift(offset);
            RRect offsetClipRRect = clipRRect.shift(offset);
            if (needsCompositing) {
                this.pushLayer(new ClipRRectLayer(offsetClipRRect, clipBehavior: clipBehavior),
                    painter, offset, childPaintBounds: offsetBounds);
            }
            else {
                this.clipRRectAndPaint(offsetClipRRect, clipBehavior, offsetBounds, () => painter(this, offset));
            }
        }

        public void pushClipPath(bool needsCompositing, Offset offset, Rect bounds, Path clipPath,
            PaintingContextCallback painter, Clip clipBehavior = Clip.antiAlias) {
            Rect offsetBounds = bounds.shift(offset);
            Path offsetClipPath = clipPath.shift(offset);
            if (needsCompositing) {
                this.pushLayer(new ClipPathLayer(clipPath: offsetClipPath, clipBehavior: clipBehavior), painter, offset,
                    childPaintBounds: offsetBounds);
            }
            else {
                this.clipPathAndPaint(offsetClipPath, clipBehavior, offsetBounds, () => painter(this, offset));
            }
        }

        public void pushTransform(bool needsCompositing, Offset offset, Matrix3 transform,
            PaintingContextCallback painter) {
            Matrix3 effectiveTransform;
            if (offset == null || offset == Offset.zero) {
                effectiveTransform = transform;
            }
            else {
                effectiveTransform = Matrix3.makeTrans(offset.dx, offset.dy);
                effectiveTransform.preConcat(transform);
                effectiveTransform.preTranslate(-offset.dx, -offset.dy);
            }

            if (needsCompositing) {
                var inverse = Matrix3.I();
                var invertible = effectiveTransform.invert(inverse);

                // it could just be "scale == 0", ignore the assertion.
                // D.assert(invertible);

                this.pushLayer(
                    new TransformLayer(effectiveTransform),
                    painter,
                    offset,
                    childPaintBounds: inverse.mapRect(this.estimatedBounds)
                );
            }
            else {
                this.canvas.save();
                this.canvas.concat(effectiveTransform);
                painter(this, offset);
                this.canvas.restore();
            }
        }

        public void pushOpacity(Offset offset, int alpha, PaintingContextCallback painter) {
            this.pushLayer(new OpacityLayer(alpha: alpha), painter, offset);
        }

        public override string ToString() {
            return
                $"{this.GetType()}#{this.GetHashCode()}(layer: {this._containerLayer}, canvas bounds: {this.estimatedBounds}";
        }
    }

    public abstract class Constraints {
        public abstract bool isTight { get; }

        public abstract bool isNormalized { get; }

        public virtual bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(this.isNormalized);
            return this.isNormalized;
        }
    }

    public delegate void RenderObjectVisitor(RenderObject child);

    public delegate void LayoutCallback<T>(T constraints) where T : Constraints;

    public class PipelineOwner {
        public PipelineOwner(
            VoidCallback onNeedVisualUpdate = null) {
            this.onNeedVisualUpdate = onNeedVisualUpdate;
        }

        public readonly VoidCallback onNeedVisualUpdate;

        public void requestVisualUpdate() {
            if (this.onNeedVisualUpdate != null) {
                this.onNeedVisualUpdate();
            }
        }

        public AbstractNodeMixinDiagnosticableTree rootNode {
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

        AbstractNodeMixinDiagnosticableTree _rootNode;

        internal List<RenderObject> _nodesNeedingLayout = new List<RenderObject>();

        public bool debugDoingLayout {
            get { return this._debugDoingLayout; }
        }

        internal bool _debugDoingLayout = false;

        public void flushLayout() {
            D.assert(() => {
                this._debugDoingLayout = true;
                return true;
            });

            try {
                while (this._nodesNeedingLayout.isNotEmpty()) {
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
            finally {
                D.assert(() => {
                    this._debugDoingLayout = false;
                    return true;
                });
            }
        }

        internal bool _debugAllowMutationsToDirtySubtrees = false;

        internal void _enableMutationsToDirtySubtrees(VoidCallback callback) {
            D.assert(this._debugDoingLayout);
            bool oldState = false;
            D.assert(() => {
                oldState = this._debugAllowMutationsToDirtySubtrees;
                this._debugAllowMutationsToDirtySubtrees = true;
                return true;
            });
            try {
                callback();
            }
            finally {
                D.assert(() => {
                    this._debugAllowMutationsToDirtySubtrees = oldState;
                    return true;
                });
            }
        }

        internal List<RenderObject> _nodesNeedingCompositingBitsUpdate = new List<RenderObject>();

        public void flushCompositingBits() {
            this._nodesNeedingCompositingBitsUpdate.Sort((a, b) => a.depth - b.depth);
            foreach (RenderObject node in this._nodesNeedingCompositingBitsUpdate) {
                if (node._needsCompositingBitsUpdate && node.owner == this) {
                    node._updateCompositingBits();
                }
            }

            this._nodesNeedingCompositingBitsUpdate.Clear();
        }

        internal List<RenderObject> _nodesNeedingPaint = new List<RenderObject>();

        public bool debugDoingPaint {
            get { return this._debugDoingPaint; }
        }

        internal bool _debugDoingPaint = false;

        public void flushPaint() {
            D.assert(() => {
                this._debugDoingPaint = true;
                return true;
            });

            try {
                var dirtyNodes = this._nodesNeedingPaint;
                this._nodesNeedingPaint = new List<RenderObject>();
                dirtyNodes.Sort((a, b) => a.depth - b.depth);
                foreach (var node in dirtyNodes) {
                    D.assert(node._layer != null);
                    if (node._needsPaint && node.owner == this) {
                        if (node._layer.attached) {
                            PaintingContext.repaintCompositedChild(node);
                        }
                        else {
                            node._skippedPaintingOnLayer();
                        }
                    }
                }
            }
            finally {
                D.assert(() => {
                    this._debugDoingPaint = false;
                    return true;
                });
            }
        }
    }

    public abstract class RenderObject : AbstractNodeMixinDiagnosticableTree, HitTestTarget {
        protected RenderObject() {
            this._needsCompositing = this.isRepaintBoundary || this.alwaysNeedsCompositing;
        }

        public ParentData parentData;

        public virtual void setupParentData(RenderObject child) {
            D.assert(this._debugCanPerformMutations);

            if (!(child.parentData is ParentData)) {
                child.parentData = new ParentData();
            }
        }

        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree childNode) {
            var child = (RenderObject) childNode;

            D.assert(this._debugCanPerformMutations);
            D.assert(child != null);
            this.setupParentData(child);
            this.markNeedsLayout();
            this.markNeedsCompositingBitsUpdate();
            base.adoptChild(child);
        }

        protected override void dropChild(AbstractNodeMixinDiagnosticableTree childNode) {
            var child = (RenderObject) childNode;

            D.assert(this._debugCanPerformMutations);
            D.assert(child != null);
            D.assert(child.parentData != null);
            child._cleanRelayoutBoundary();
            child.parentData.detach();
            child.parentData = null;
            base.dropChild(child);
            this.markNeedsLayout();
            this.markNeedsCompositingBitsUpdate();
        }

        public virtual void visitChildren(RenderObjectVisitor visitor) {
        }

        public object debugCreator;

        void _debugReportException(string method, Exception exception) {
            UIWidgetsError.reportError(new UIWidgetsErrorDetailsForRendering(
                exception: exception,
                library: "rendering library",
                context: "during " + method,
                renderObject: this,
                informationCollector: information => {
                    information.AppendLine(
                        "The following RenderObject was being processed when the exception was fired:");
                    information.AppendLine("  " + this.toStringShallow(joiner: "\n  "));
                    var descendants = new List<string>();
                    const int maxDepth = 5;
                    int depth = 0;
                    const int maxLines = 25;
                    int lines = 0;
                    RenderObjectVisitor visitor = null;
                    visitor = new RenderObjectVisitor((RenderObject child) => {
                        if (lines < maxLines) {
                            depth += 1;
                            descendants.Add(new string(' ', 2 * depth) + child);
                            if (depth < maxDepth) {
                                child.visitChildren(visitor);
                            }

                            depth -= 1;
                        }
                        else if (lines == maxLines) {
                            descendants.Add("  ...(descendants list truncated after " + lines + " lines)");
                        }

                        lines += 1;
                    });
                    this.visitChildren(visitor);
                    if (lines > 1) {
                        information.AppendLine(
                            "This RenderObject had the following descendants (showing up to depth " +
                            maxDepth + "):");
                    }
                    else if (descendants.Count == 1) {
                        information.AppendLine("This RenderObject had the following child:");
                    }
                    else {
                        information.AppendLine("This RenderObject has no descendants.");
                    }

                    information.Append(string.Join("\n", descendants.ToArray()));
                }
            ));
        }

        public bool debugDoingThisResize {
            get { return this._debugDoingThisResize; }
        }

        bool _debugDoingThisResize = false;

        public bool debugDoingThisLayout {
            get { return this._debugDoingThisLayout; }
        }

        bool _debugDoingThisLayout = false;

        public static RenderObject debugActiveLayout {
            get { return _debugActiveLayout; }
        }

        static RenderObject _debugActiveLayout;

        public bool debugCanParentUseSize {
            get { return this._debugCanParentUseSize; }
        }

        bool _debugCanParentUseSize;

        bool _debugMutationsLocked = false;

        bool _debugCanPerformMutations {
            get {
                bool result = true;
                D.assert(() => {
                    RenderObject node = this;
                    while (true) {
                        if (node._doingThisLayoutWithCallback) {
                            result = true;
                            break;
                        }

                        if (this.owner != null && this.owner._debugAllowMutationsToDirtySubtrees && node._needsLayout) {
                            result = true;
                            break;
                        }

                        if (node._debugMutationsLocked) {
                            result = false;
                            break;
                        }

                        if (!(node.parent is RenderObject)) {
                            result = true;
                            break;
                        }

                        node = (RenderObject) node.parent;
                    }

                    return true;
                });
                return result;
            }
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

        public bool debugNeedsLayout {
            get {
                bool result = false;
                D.assert(() => {
                    result = this._needsLayout;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsLayout = true;

        public RenderObject _relayoutBoundary;
        bool _doingThisLayoutWithCallback = false;

        public Constraints constraints {
            get { return this._constraints; }
        }

        Constraints _constraints;

        protected abstract void debugAssertDoesMeetConstraints();

        internal static bool debugCheckingIntrinsics = false;

        bool _debugSubtreeRelayoutRootAlreadyMarkedNeedsLayout() {
            if (this._relayoutBoundary == null) {
                return true;
            }

            RenderObject node = this;
            while (node != this._relayoutBoundary) {
                D.assert(node._relayoutBoundary == this._relayoutBoundary);
                D.assert(node.parent != null);
                node = (RenderObject) node.parent;
                if ((!node._needsLayout) && (!node._debugDoingThisLayout)) {
                    return false;
                }
            }

            D.assert(node._relayoutBoundary == node);
            return true;
        }

        public virtual void markNeedsLayout() {
            D.assert(this._debugCanPerformMutations);
            if (this._needsLayout) {
                D.assert(this._debugSubtreeRelayoutRootAlreadyMarkedNeedsLayout());
                return;
            }

            D.assert(this._relayoutBoundary != null);
            if (this._relayoutBoundary != this) {
                this.markParentNeedsLayout();
            }
            else {
                this._needsLayout = true;
                if (this.owner != null) {
                    D.assert(() => {
                        if (D.debugPrintMarkNeedsLayoutStacks) {
                            Debug.Log("markNeedsLayout() called for " + this);
                        }

                        return true;
                    });

                    this.owner._nodesNeedingLayout.Add(this);
                    this.owner.requestVisualUpdate();
                }
            }
        }

        protected void markParentNeedsLayout() {
            this._needsLayout = true;

            RenderObject parent = (RenderObject) this.parent;
            if (!this._doingThisLayoutWithCallback) {
                parent.markNeedsLayout();
            }
            else {
                D.assert(parent._debugDoingThisLayout);
            }
        }

        public void markNeedsLayoutForSizedByParentChange() {
            this.markNeedsLayout();
            this.markParentNeedsLayout();
        }

        void _cleanRelayoutBoundary() {
            if (this._relayoutBoundary != this) {
                this._relayoutBoundary = null;
                this._needsLayout = true;
                this.visitChildren(child => { child._cleanRelayoutBoundary(); });
            }
        }

        public void scheduleInitialLayout() {
            D.assert(this.attached);
            D.assert(!(this.parent is RenderObject));
            D.assert(!this.owner._debugDoingLayout);
            D.assert(this._relayoutBoundary == null);

            this._relayoutBoundary = this;
            D.assert(() => {
                this._debugCanParentUseSize = false;
                return true;
            });
            this.owner._nodesNeedingLayout.Add(this);
        }

        internal void _layoutWithoutResize() {
            D.assert(this._relayoutBoundary == this);
            RenderObject debugPreviousActiveLayout = null;
            D.assert(!this._debugMutationsLocked);
            D.assert(!this._doingThisLayoutWithCallback);
            D.assert(() => {
                this._debugMutationsLocked = true;
                this._debugDoingThisLayout = true;
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = this;
                if (D.debugPrintLayouts) {
                    Debug.Log("Laying out (without resize) " + this);
                }

                return true;
            });

            try {
                this.performLayout();
            }
            catch (Exception ex) {
                this._debugReportException("performLayout", ex);
            }

            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                this._debugDoingThisLayout = false;
                this._debugMutationsLocked = false;
                return true;
            });

            this._needsLayout = false;
            this.markNeedsPaint();
        }

        public void layout(Constraints constraints, bool parentUsesSize = false) {
            D.assert(constraints != null);
            D.assert(constraints.debugAssertIsValid(
                isAppliedConstraint: true,
                informationCollector: information => {
//                final List<String> stack = StackTrace.current.toString().split('\n');
//                int targetFrame;
//                final Pattern layoutFramePattern = RegExp(r'^#[0-9]+ +RenderObject.layout \(');
//                for (int i = 0; i < stack.length; i += 1) {
//                    if (layoutFramePattern.matchAsPrefix(stack[i]) != null) {
//                        targetFrame = i + 1;
//                        break;
//                    }
//                }
//                if (targetFrame != null && targetFrame < stack.length) {
//                    information.writeln(
//                        'These invalid constraints were provided to $runtimeType\'s layout() '
//                    'function by the following function, which probably computed the '
//                    'invalid constraints in question:'
//                        );
//                    final Pattern targetFramePattern = RegExp(r'^#[0-9]+ +(.+)$');
//                    final Match targetFrameMatch = targetFramePattern.matchAsPrefix(stack[targetFrame]);
//                    if (targetFrameMatch != null && targetFrameMatch.groupCount > 0) {
//                        information.writeln('  ${targetFrameMatch.group(1)}');
//                    } else {
//                        information.writeln(stack[targetFrame]);
//                    }
//                }
                }));
            D.assert(!this._debugDoingThisResize);
            D.assert(!this._debugDoingThisLayout);

            RenderObject relayoutBoundary;
            if (!parentUsesSize || this.sizedByParent || constraints.isTight || !(this.parent is RenderObject)) {
                relayoutBoundary = this;
            }
            else {
                RenderObject parent = (RenderObject) this.parent;
                relayoutBoundary = parent._relayoutBoundary;
            }

            D.assert(() => {
                this._debugCanParentUseSize = parentUsesSize;
                return true;
            });

            if (!this._needsLayout && Equals(constraints, this._constraints) &&
                relayoutBoundary == this._relayoutBoundary) {
                D.assert(() => {
                    this._debugDoingThisResize = this.sizedByParent;
                    this._debugDoingThisLayout = !this.sizedByParent;
                    RenderObject debugPreviousActiveLayout1 = _debugActiveLayout;
                    _debugActiveLayout = this;
                    this.debugResetSize();
                    _debugActiveLayout = debugPreviousActiveLayout1;
                    this._debugDoingThisLayout = false;
                    this._debugDoingThisResize = false;
                    return true;
                });

                return;
            }

            this._constraints = constraints;
            this._relayoutBoundary = relayoutBoundary;

            D.assert(!this._debugMutationsLocked);
            D.assert(!this._doingThisLayoutWithCallback);
            D.assert(() => {
                this._debugMutationsLocked = true;
                if (D.debugPrintLayouts) {
                    Debug.Log("Laying out (" + (this.sizedByParent ? "with separate resize" : "with resize allowed") +
                              ") " + this);
                }

                return true;
            });

            if (this.sizedByParent) {
                D.assert(() => {
                    this._debugDoingThisResize = true;
                    return true;
                });

                try {
                    this.performResize();
                    D.assert(() => {
                        this.debugAssertDoesMeetConstraints();
                        return true;
                    });
                }
                catch (Exception ex) {
                    this._debugReportException("performResize", ex);
                }

                D.assert(() => {
                    this._debugDoingThisResize = false;
                    return true;
                });
            }

            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                this._debugDoingThisLayout = true;
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = this;
                return true;
            });

            try {
                this.performLayout();
                D.assert(() => {
                    this.debugAssertDoesMeetConstraints();
                    return true;
                });
            }
            catch (Exception ex) {
                this._debugReportException("performLayout", ex);
            }

            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                this._debugDoingThisLayout = false;
                this._debugMutationsLocked = false;
                return true;
            });

            this._needsLayout = false;
            this.markNeedsPaint();
        }

        protected virtual void debugResetSize() {
        }

        protected virtual bool sizedByParent {
            get { return false; }
        }

        protected abstract void performResize();

        protected abstract void performLayout();

        protected void invokeLayoutCallback<T>(LayoutCallback<T> callback) where T : Constraints {
            D.assert(this._debugMutationsLocked);
            D.assert(this._debugDoingThisLayout);
            D.assert(!this._doingThisLayoutWithCallback);

            this._doingThisLayoutWithCallback = true;
            try {
                this.owner._enableMutationsToDirtySubtrees(() => { callback((T) this.constraints); });
            }
            finally {
                this._doingThisLayoutWithCallback = false;
            }
        }

        public bool debugDoingThisPaint {
            get { return this._debugDoingThisPaint; }
        }

        bool _debugDoingThisPaint = false;

        public static RenderObject debugActivePaint {
            get { return _debugActivePaint; }
        }

        static RenderObject _debugActivePaint;

        public virtual bool isRepaintBoundary {
            get { return false; }
        }

        public virtual void debugRegisterRepaintBoundaryPaint(bool includedParent = true, bool includedChild = false) {
        }

        protected virtual bool alwaysNeedsCompositing {
            get { return false; }
        }

        internal OffsetLayer _layer;

        public OffsetLayer layer {
            get {
                D.assert(this.isRepaintBoundary,
                    () => "You can only access RenderObject.layer for render objects that are repaint boundaries.");
                D.assert(!this._needsPaint);

                return this._layer;
            }
        }

        public OffsetLayer debugLayer {
            get {
                OffsetLayer result = null;
                D.assert(() => {
                    result = this._layer;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsCompositingBitsUpdate = false;

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

            D.assert(() => {
                var parent = this.parent;
                if (parent is RenderObject) {
                    return ((RenderObject) parent)._needsCompositing;
                }

                return true;
            });

            if (this.owner != null) {
                this.owner._nodesNeedingCompositingBitsUpdate.Add(this);
            }
        }

        bool _needsCompositing;

        public bool needsCompositing {
            get {
                D.assert(!this._needsCompositingBitsUpdate);
                return this._needsCompositing;
            }
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

        bool debugNeedsPaint {
            get {
                bool result = false;
                D.assert(() => {
                    result = this._needsPaint;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsPaint = true;

        public void markNeedsPaint() {
            D.assert(this.owner == null || !this.owner.debugDoingPaint);

            if (this._needsPaint) {
                return;
            }

            this._needsPaint = true;
            if (this.isRepaintBoundary) {
                D.assert(() => {
                    if (D.debugPrintMarkNeedsPaintStacks) {
                        Debug.Log("markNeedsPaint() called for " + this);
                    }

                    return true;
                });

                D.assert(this._layer != null);

                if (this.owner != null) {
                    this.owner._nodesNeedingPaint.Add(this);
                    this.owner.requestVisualUpdate();
                }
            }
            else if (this.parent is RenderObject) {
                D.assert(this._layer == null);
                var parent = (RenderObject) this.parent;
                parent.markNeedsPaint();
            }
            else {
                D.assert(() => {
                    if (D.debugPrintMarkNeedsPaintStacks) {
                        Debug.Log("markNeedsPaint() called for " + this + " (root of render tree)");
                    }

                    return true;
                });

                if (this.owner != null) {
                    this.owner.requestVisualUpdate();
                }
            }
        }

        internal void _skippedPaintingOnLayer() {
            D.assert(this.attached);
            D.assert(this.isRepaintBoundary);
            D.assert(this._needsPaint);
            D.assert(this._layer != null);
            D.assert(!this._layer.attached);

            var ancestor = this.parent;
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
            D.assert(rootLayer.attached);
            D.assert(this.attached);
            D.assert(!(this.parent is RenderObject));
            D.assert(!this.owner._debugDoingPaint);
            D.assert(this.isRepaintBoundary);
            D.assert(this._layer == null);

            this._layer = (OffsetLayer) rootLayer;
            D.assert(this._needsPaint);
            this.owner._nodesNeedingPaint.Add(this);
        }

        public void replaceRootLayer(OffsetLayer rootLayer) {
            D.assert(rootLayer.attached);
            D.assert(this.attached);
            D.assert(!(this.parent is RenderObject));
            D.assert(!this.owner._debugDoingPaint);
            D.assert(this.isRepaintBoundary);
            D.assert(this._layer != null);


            this._layer.detach();
            this._layer = rootLayer;
            this.markNeedsPaint();
        }

        internal void _paintWithContext(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this._debugDoingThisPaint) {
                    throw new UIWidgetsError(
                        "Tried to paint a RenderObject reentrantly.\n" +
                        "The following RenderObject was already being painted when it was " +
                        "painted again:\n" +
                        "  " + this.toStringShallow(joiner: "\n    ") + "\n" +
                        "Since this typically indicates an infinite recursion, it is " +
                        "disallowed."
                    );
                }

                return true;
            });

            if (this._needsLayout) {
                return;
            }

            D.assert(() => {
                if (this._needsCompositingBitsUpdate) {
                    throw new UIWidgetsError(
                        "Tried to paint a RenderObject before its compositing bits were " +
                        "updated.\n" +
                        "The following RenderObject was marked as having dirty compositing " +
                        "bits at the time that it was painted:\n" +
                        "  " + this.toStringShallow(joiner: "\n    ") + "\n" +
                        "A RenderObject that still has dirty compositing bits cannot be " +
                        "painted because this indicates that the tree has not yet been " +
                        "properly configured for creating the layer tree.\n" +
                        "This usually indicates an error in the Flutter framework itself."
                    );
                }

                return true;
            });

            RenderObject debugLastActivePaint = null;
            D.assert(() => {
                this._debugDoingThisPaint = true;
                debugLastActivePaint = _debugActivePaint;
                _debugActivePaint = this;
                D.assert(!this.isRepaintBoundary || this._layer != null);
                return true;
            });

            this._needsPaint = false;
            try {
                this.paint(context, offset);
                D.assert(!this._needsLayout);
                D.assert(!this._needsPaint);
            }
            catch (Exception ex) {
                this._debugReportException("paint", ex);
            }

            D.assert(() => {
                this.debugPaint(context, offset);
                _debugActivePaint = debugLastActivePaint;
                this._debugDoingThisPaint = false;
                return true;
            });
        }

        public abstract Rect paintBounds { get; }

        public virtual void debugPaint(PaintingContext context, Offset offset) {
        }

        public virtual void paint(PaintingContext context, Offset offset) {
        }

        public virtual void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(child.parent == this);
        }

        public Matrix3 getTransformTo(RenderObject ancestor) {
            D.assert(this.attached);

            if (ancestor == null) {
                var rootNode = this.owner.rootNode;
                if (rootNode is RenderObject) {
                    ancestor = (RenderObject) rootNode;
                }
            }

            var renderers = new List<RenderObject>();
            for (RenderObject renderer = this; renderer != ancestor; renderer = (RenderObject) renderer.parent) {
                D.assert(renderer != null);
                renderers.Add(renderer);
            }

            var transform = Matrix3.I();
            for (int index = renderers.Count - 1; index > 0; index -= 1) {
                renderers[index].applyPaintTransform(renderers[index - 1], transform);
            }

            return transform;
        }

        public virtual Rect describeApproximatePaintClip(RenderObject child) {
            return null;
        }

        public abstract Rect semanticBounds { get; }

        public virtual void handleEvent(PointerEvent evt, HitTestEntry entry) {
        }

        public override string toStringShort() {
            string header = Diagnostics.describeIdentity(this);
            if (this._relayoutBoundary != null && this._relayoutBoundary != this) {
                int count = 1;
                RenderObject target = (RenderObject) this.parent;
                while (target != null && target != this._relayoutBoundary) {
                    target = (RenderObject) target.parent;
                    count += 1;
                }

                header += " relayoutBoundary=up" + count;
            }

            if (this._needsLayout) {
                header += " NEEDS-LAYOUT";
            }

            if (this._needsPaint) {
                header += " NEEDS-PAINT";
            }

            if (!this.attached) {
                header += " DETACHED";
            }

            return header;
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            return this.toStringShort();
        }

        public override string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = "",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = null;
                return true;
            });
            string result = base.toStringDeep(
                prefixLineOne: prefixLineOne,
                prefixOtherLines: prefixOtherLines,
                minLevel: minLevel
            );
            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                return true;
            });
            return result;
        }

        public override string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = null;
                return true;
            });
            string result = base.toStringShallow(joiner: joiner, minLevel: minLevel);
            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                return true;
            });
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>(
                "creator", this.debugCreator, defaultValue: Diagnostics.kNullDefaultValue,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<ParentData>("parentData", this.parentData,
                tooltip: this._debugCanParentUseSize ? "can use size" : null, missingIfNull: true));
            properties.add(new DiagnosticsProperty<Constraints>("constraints", this.constraints, missingIfNull: true));
            properties.add(new DiagnosticsProperty<OffsetLayer>("layer", this._layer,
                defaultValue: Diagnostics.kNullDefaultValue));
        }

        public virtual void showOnScreen(
            RenderObject descendant = null,
            Rect rect = null,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            duration = duration ?? TimeSpan.Zero;
            curve = curve ?? Curves.ease;

            if (this.parent is RenderObject) {
                RenderObject renderParent = (RenderObject) this.parent;
                renderParent.showOnScreen(
                    descendant: descendant ?? this,
                    rect: rect,
                    duration: duration,
                    curve: curve
                );
            }
        }
    }

    public interface RenderObjectWithChildMixin {
        bool debugValidateChild(RenderObject child);
        RenderObject child { get; set; }
    }

    public interface RenderObjectWithChildMixin<ChildType> : RenderObjectWithChildMixin
        where ChildType : RenderObject {
        new ChildType child { get; set; }
    }

    public interface ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        ChildType previousSibling { get; set; }
        ChildType nextSibling { get; set; }
    }

    public interface ContainerRenderObjectMixin {
        int childCount { get; }
        bool debugValidateChild(RenderObject child);
        void insert(RenderObject child, RenderObject after = null);
        void remove(RenderObject child);
        void move(RenderObject child, RenderObject after = null);
        RenderObject firstChild { get; }
        RenderObject lastChild { get; }
        RenderObject childBefore(RenderObject child);
        RenderObject childAfter(RenderObject child);
    }

    public class UIWidgetsErrorDetailsForRendering : UIWidgetsErrorDetails {
        public UIWidgetsErrorDetailsForRendering(
            Exception exception = null,
            string library = null,
            string context = null,
            RenderObject renderObject = null,
            InformationCollector informationCollector = null,
            bool silent = false
        ) : base(
            exception: exception,
            library: library,
            context: context,
            informationCollector: informationCollector,
            silent: silent
        ) {
            this.renderObject = renderObject;
        }

        public readonly RenderObject renderObject;
    }
}