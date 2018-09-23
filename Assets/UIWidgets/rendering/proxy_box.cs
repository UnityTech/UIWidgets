using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.rendering {
    public class RenderProxyBox : RenderProxyBoxMixinRenderObjectWithChildMixinRenderBox<RenderBox> {
        public RenderProxyBox(RenderBox child = null) {
            this.child = child;
        }
    }

    public enum HitTestBehavior {
        deferToChild,
        opaque,
        translucent,
    }

    public abstract class RenderProxyBoxWithHitTestBehavior : RenderProxyBox {
        protected RenderProxyBoxWithHitTestBehavior(
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(child) {
            this.behavior = behavior;
        }

        public HitTestBehavior behavior;

        public override bool hitTest(HitTestResult result, Offset position = null) {
            bool hitTarget = false;
            if (this.size.contains(position)) {
                hitTarget = this.hitTestChildren(result, position: position) || this.hitTestSelf(position);
                if (hitTarget || this.behavior == HitTestBehavior.translucent) {
                    result.add(new BoxHitTestEntry(this, position));
                }
            }

            return hitTarget;
        }

        protected override bool hitTestSelf(Offset position) {
            return this.behavior == HitTestBehavior.opaque;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<HitTestBehavior>(
                "behavior", this.behavior, defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public class RenderConstrainedBox : RenderProxyBox {
        public RenderConstrainedBox(
            RenderBox child = null,
            BoxConstraints additionalConstraints = null) : base(child) {
            D.assert(additionalConstraints != null);
            D.assert(additionalConstraints.debugAssertIsValid());

            this._additionalConstraints = additionalConstraints;
        }

        public BoxConstraints additionalConstraints {
            get { return this._additionalConstraints; }
            set {
                D.assert(value != null);
                D.assert(value.debugAssertIsValid());

                if (this._additionalConstraints == value) {
                    return;
                }

                this._additionalConstraints = value;
                this.markNeedsLayout();
            }
        }

        BoxConstraints _additionalConstraints;

        protected override double computeMinIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.minWidth;
            }

            double width = base.computeMinIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.maxWidth;
            }

            double width = base.computeMaxIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMinIntrinsicHeight(width);
            D.assert(height.isFinite());

            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMaxIntrinsicHeight(width);
            D.assert(height.isFinite());

            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._additionalConstraints.enforce(this.constraints), parentUsesSize: true);
                this.size = this.child.size;
            } else {
                this.size = this._additionalConstraints.enforce(this.constraints).constrain(Size.zero);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(() => {
                if (this.child == null || this.child.size.isEmpty) {
                    var paint = new Paint {
                        color = new Color(0x90909090)
                    };
                    context.canvas.drawRect(offset & this.size, BorderWidth.zero, BorderRadius.zero, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<BoxConstraints>("additionalConstraints", this.additionalConstraints));
        }
    }

    public class RenderLimitedBox : RenderProxyBox {
        public RenderLimitedBox(
            RenderBox child = null,
            double maxWidth = double.PositiveInfinity,
            double maxHeight = double.PositiveInfinity
        ) : base(child) {
            D.assert(maxWidth >= 0.0);
            D.assert(maxHeight >= 0.0);

            this._maxWidth = maxWidth;
            this._maxHeight = maxHeight;
        }

        public double maxWidth {
            get { return this._maxWidth; }
            set {
                D.assert(value >= 0.0);
                if (this._maxWidth == value) {
                    return;
                }

                this._maxWidth = value;
                this.markNeedsLayout();
            }
        }

        double _maxWidth;

        public double maxHeight {
            get { return this._maxHeight; }
            set {
                D.assert(value >= 0.0);
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        double _maxHeight;

        BoxConstraints _limitConstraints(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: constraints.minWidth,
                maxWidth: constraints.hasBoundedWidth
                    ? constraints.maxWidth
                    : constraints.constrainWidth(this.maxWidth),
                minHeight: constraints.minHeight,
                maxHeight: constraints.hasBoundedHeight
                    ? constraints.maxHeight
                    : constraints.constrainHeight(this.maxHeight)
            );
        }

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._limitConstraints(this.constraints), parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
            } else {
                this.size = this._limitConstraints(this.constraints).constrain(Size.zero);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("maxWidth", this.maxWidth, defaultValue: double.PositiveInfinity));
            properties.add(new DoubleProperty("maxHeight", this.maxHeight, defaultValue: double.PositiveInfinity));
        }
    }

    public class RenderOpacity : RenderProxyBox {
        public RenderOpacity(double opacity = 1.0, RenderBox child = null) : base(child) {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            this._opacity = opacity;
            this._alpha = _getAlphaFromOpacity(opacity);
        }

        protected override bool alwaysNeedsCompositing {
            get { return this.child != null && (this._alpha != 0 && this._alpha != 255); }
        }

        int _alpha;

        static int _getAlphaFromOpacity(double opacity) {
            return (opacity * 255).round();
        }

        public double opacity {
            get { return this._opacity; }
            set {
                D.assert(value >= 0.0 && value <= 1.0);
                if (this._opacity == value) {
                    return;
                }

                bool didNeedCompositing = this.alwaysNeedsCompositing;

                this._opacity = value;
                this._alpha = _getAlphaFromOpacity(this._opacity);

                if (didNeedCompositing != this.alwaysNeedsCompositing) {
                    this.markNeedsCompositingBitsUpdate();
                }

                this.markNeedsPaint();
            }
        }

        double _opacity;

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                if (this._alpha == 0) {
                    return;
                }
            }

            if (this._alpha == 255) {
                context.paintChild(this.child, offset);
                return;
            }

            D.assert(this.needsCompositing);
            context.pushOpacity(offset, this._alpha, base.paint);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("opacity", this.opacity));
        }
    }

    public enum DecorationPosition {
        background,
        foreground,
    }

    public class RenderDecoratedBox : RenderProxyBox {
        public RenderDecoratedBox(
            Decoration decoration = null,
            DecorationPosition position = DecorationPosition.background,
            ImageConfiguration configuration = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(decoration != null);
            this._decoration = decoration;
            this._position = position;
            this._configuration = configuration ?? ImageConfiguration.empty;
        }

        BoxPainter _painter;

        public Decoration decoration {
            get { return this._decoration; }
            set {
                D.assert(value != null);
                if (value == this._decoration) {
                    return;
                }

                if (this._painter != null) {
                    this._painter.dispose();
                    this._painter = null;
                }

                this._decoration = value;
                this.markNeedsPaint();
            }
        }

        Decoration _decoration;

        public DecorationPosition position {
            get { return this._position; }
            set {
                if (value == this._position) {
                    return;
                }

                this._position = value;
                this.markNeedsPaint();
            }
        }

        DecorationPosition _position;

        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                D.assert(value != null);
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public override void detach() {
            if (this._painter != null) {
                this._painter.dispose();
                this._painter = null;
            }

            base.detach();
            this.markNeedsPaint();
        }

        public override void paint(PaintingContext context, Offset offset) {
            this._painter = this._painter ?? this._decoration.createBoxPainter(this.markNeedsPaint);
            var filledConfiguration = this.configuration.copyWith(size: this.size);

            if (this.position == DecorationPosition.background) {
                int debugSaveCount = 0;
                D.assert(() => {
                    debugSaveCount = context.canvas.getSaveCount();
                    return true;
                });

                this._painter.paint(context.canvas, offset, filledConfiguration);

                D.assert(() => {
                    if (debugSaveCount != context.canvas.getSaveCount()) {
                        throw new UIWidgetsError(
                            this._decoration.GetType() + " painter had mismatching save and restore calls.\n" +
                            "Before painting the decoration, the canvas save count was $debugSaveCount. " +
                            "After painting it, the canvas save count was " + context.canvas.getSaveCount() + ". " +
                            "Every call to save() or saveLayer() must be matched by a call to restore().\n" +
                            "The decoration was:\n" +
                            "  " + this.decoration + "\n" +
                            "The painter was:\n" +
                            "  " + this._painter
                        );
                    }

                    return true;
                });

                if (this.decoration.isComplex) {
                    context.setIsComplexHint();
                }
            }

            base.paint(context, offset);

            if (this.position == DecorationPosition.foreground) {
                this._painter.paint(context.canvas, offset, filledConfiguration);
                if (this.decoration.isComplex) {
                    context.setIsComplexHint();
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(this._decoration.toDiagnosticsNode(name: "decoration"));
            properties.add(new DiagnosticsProperty<ImageConfiguration>("configuration", this.configuration));
        }
    }

    public class RenderTransform : RenderProxyBox {
        public RenderTransform(
            Matrix4x4 transform,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            RenderBox child = null
        ) : base(child) {
            this.transform = transform;
            this.origin = origin;
            this.alignment = alignment;
            this.transformHitTests = transformHitTests;
        }

        public Offset origin {
            get { return this._origin; }
            set {
                if (this._origin == value) {
                    return;
                }

                this._origin = value;
                this.markNeedsPaint();
            }
        }

        Offset _origin;

        public Alignment alignment {
            get { return this._alignment; }
            set {
                if (this._alignment == value) {
                    return;
                }

                this._alignment = value;
                this.markNeedsPaint();
            }
        }

        Alignment _alignment;

        public bool transformHitTests;

        public Matrix4x4 transform {
            set {
                if (this._transform == value) {
                    return;
                }

                this._transform = value;
                this.markNeedsPaint();
            }
        }

        Matrix4x4 _transform;

        public void setIdentity() {
            this._transform = Matrix4x4.identity;
            this.markNeedsPaint();
        }

        public void rotateX(double degrees) {
            this._transform = Matrix4x4.Rotate(Quaternion.Euler((float) degrees, 0, 0)) * this._transform;
            this.markNeedsPaint();
        }

        public void rotateY(double degrees) {
            this._transform = Matrix4x4.Rotate(Quaternion.Euler(0, (float) degrees, 0)) * this._transform;
            this.markNeedsPaint();
        }

        public void rotateZ(double degrees) {
            this._transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, (float) degrees)) * this._transform;
            this.markNeedsPaint();
        }

        public void translate(double x, double y = 0.0, double z = 0.0) {
            this._transform = Matrix4x4.Translate(new Vector3((float) x, (float) y, (float) z)) * this._transform;
            this.markNeedsPaint();
        }

        public void scale(double x, double y, double z) {
            this._transform = Matrix4x4.Scale(new Vector3((float) x, (float) y, (float) z)) * this._transform;
            this.markNeedsPaint();
        }

        Matrix4x4 _effectiveTransform {
            get {
                Alignment resolvedAlignment = this.alignment;
                if (this._origin == null && resolvedAlignment == null) {
                    return this._transform;
                }

                var result = Matrix4x4.identity;
                if (this._origin != null) {
                    result = Matrix4x4.Translate(new Vector2((float) this._origin.dx, (float) this._origin.dy)) *
                             result;
                }

                Offset translation = null;
                if (resolvedAlignment != null) {
                    translation = resolvedAlignment.alongSize(this.size);
                    result = Matrix4x4.Translate(new Vector2((float) translation.dx, (float) translation.dy)) * result;
                }

                result = this._transform * result;

                if (resolvedAlignment != null) {
                    result = Matrix4x4.Translate(new Vector2((float) -translation.dx, (float) -translation.dy)) *
                             result;
                }

                if (this._origin != null) {
                    result = Matrix4x4.Translate(new Vector2((float) -this._origin.dx, (float) -this._origin.dy)) *
                             result;
                }

                return result;
            }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            return this.hitTestChildren(result, position: position);
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            if (this.transformHitTests) {
                var transform = this._effectiveTransform;
                if (transform.determinant == 0) {
                    return false;
                }

                position = MatrixUtils.transformPoint(transform.inverse, position);
            }

            return base.hitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                var transform = this._effectiveTransform;
                Offset childOffset = MatrixUtils.getAsTranslation(ref transform);
                if (childOffset == null) {
                    context.pushTransform(this.needsCompositing, offset, transform, base.paint);
                } else {
                    base.paint(context, offset + childOffset);
                }
            }
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            transform = this._effectiveTransform * transform;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Matrix4x4>("transform matrix", this._transform));
            properties.add(new DiagnosticsProperty<Offset>("origin", this.origin));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
            properties.add(new DiagnosticsProperty<bool>("transformHitTests", this.transformHitTests));
        }
    }

    public delegate void PointerDownEventListener(PointerDownEvent evt);

    public delegate void PointerMoveEventListener(PointerMoveEvent evt);

    public delegate void PointerUpEventListener(PointerUpEvent evt);

    public delegate void PointerCancelEventListener(PointerCancelEvent evt);

    public class RenderPointerListener : RenderProxyBoxWithHitTestBehavior {
        public RenderPointerListener(
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(behavior: behavior, child: child) {
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
        }

        public PointerDownEventListener onPointerDown;

        public PointerMoveEventListener onPointerMove;

        public PointerUpEventListener onPointerUp;

        public PointerCancelEventListener onPointerCancel;

        protected override void performResize() {
            this.size = this.constraints.biggest;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(evt, entry));

            if (this.onPointerDown != null && evt is PointerDownEvent) {
                this.onPointerDown((PointerDownEvent) evt);
                return;
            }

            if (this.onPointerMove != null && evt is PointerMoveEvent) {
                this.onPointerMove((PointerMoveEvent) evt);
                return;
            }

            if (this.onPointerUp != null && evt is PointerUpEvent) {
                this.onPointerUp((PointerUpEvent) evt);
                return;
            }

            if (this.onPointerCancel != null && evt is PointerCancelEvent) {
                this.onPointerCancel((PointerCancelEvent) evt);
                return;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            var listeners = new List<string>();
            if (this.onPointerDown != null)
                listeners.Add("down");
            if (this.onPointerMove != null)
                listeners.Add("move");
            if (this.onPointerUp != null)
                listeners.Add("up");
            if (this.onPointerCancel != null)
                listeners.Add("cancel");
            if (listeners.isEmpty()) {
                listeners.Add("<none>");
            }

            properties.add(new EnumerableProperty<string>("listeners", listeners));
        }
    }

    public class RenderRepaintBoundary : RenderProxyBox {
        public RenderRepaintBoundary(
            RenderBox child = null
        ) : base(child) {
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        public int debugSymmetricPaintCount {
            get { return this._debugSymmetricPaintCount; }
        }

        int _debugSymmetricPaintCount = 0;

        public int debugAsymmetricPaintCount {
            get { return this._debugAsymmetricPaintCount; }
        }

        int _debugAsymmetricPaintCount = 0;

        public void debugResetMetrics() {
            D.assert(() => {
                this._debugSymmetricPaintCount = 0;
                this._debugAsymmetricPaintCount = 0;
                return true;
            });
        }

        public override void debugRegisterRepaintBoundaryPaint(bool includedParent = true, bool includedChild = false) {
            D.assert(() => {
                if (includedParent && includedChild) {
                    this._debugSymmetricPaintCount += 1;
                } else {
                    this._debugAsymmetricPaintCount += 1;
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            bool inReleaseMode = true;
            D.assert(() => {
                inReleaseMode = false;
                if (this.debugSymmetricPaintCount + this.debugAsymmetricPaintCount == 0) {
                    properties.add(new MessageProperty("usefulness ratio", "no metrics collected yet (never painted)"));
                } else {
                    double fraction = (double) this.debugAsymmetricPaintCount /
                                      (this.debugSymmetricPaintCount + this.debugAsymmetricPaintCount);

                    string diagnosis;
                    if (this.debugSymmetricPaintCount + this.debugAsymmetricPaintCount < 5) {
                        diagnosis = "insufficient data to draw conclusion (less than five repaints)";
                    } else if (fraction > 0.9) {
                        diagnosis = "this is an outstandingly useful repaint boundary and should definitely be kept";
                    } else if (fraction > 0.5) {
                        diagnosis = "this is a useful repaint boundary and should be kept";
                    } else if (fraction > 0.30) {
                        diagnosis =
                            "this repaint boundary is probably useful, but maybe it would be more useful in tandem with adding more repaint boundaries elsewhere";
                    } else if (fraction > 0.1) {
                        diagnosis = "this repaint boundary does sometimes show value, though currently not that often";
                    } else if (this.debugAsymmetricPaintCount == 0) {
                        diagnosis = "this repaint boundary is astoundingly ineffectual and should be removed";
                    } else {
                        diagnosis = "this repaint boundary is not very effective and should probably be removed";
                    }

                    properties.add(new PercentProperty("metrics", fraction, unit: "useful",
                        tooltip: this.debugSymmetricPaintCount + " bad vs " + this.debugAsymmetricPaintCount + " good"));
                    properties.add(new MessageProperty("diagnosis", diagnosis));
                }

                return true;
            });
            if (inReleaseMode) {
                properties.add(DiagnosticsNode.message("(run in checked mode to collect repaint boundary statistics)"));
            }
        }
    }

    public class RenderIgnorePointer : RenderProxyBox {
        public RenderIgnorePointer(
            RenderBox child = null,
            bool ignoring = true
        ) : base(child) {
            this._ignoring = ignoring;
        }

        public bool ignoring {
            get { return this._ignoring; }
            set {
                if (value == this._ignoring) {
                    return;
                }

                this._ignoring = value;
            }
        }

        bool _ignoring;

        public override bool hitTest(HitTestResult result, Offset position = null) {
            return this.ignoring ? false : base.hitTest(result, position: position);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", this.ignoring));
        }
    }
}