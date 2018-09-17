using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

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
            this._additionalConstraints = additionalConstraints;
        }

        public BoxConstraints additionalConstraints {
            get { return this._additionalConstraints; }

            set {
                if (this._additionalConstraints == value) {
                    return;
                }

                this._additionalConstraints = value;
                this.markNeedsLayout();
            }
        }

        public BoxConstraints _additionalConstraints;

        public override double computeMinIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.minWidth;
            }

            double width = base.computeMinIntrinsicWidth(height);
            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        public override double computeMaxIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.maxWidth;
            }

            double width = base.computeMaxIntrinsicWidth(height);
            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        public override double computeMinIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMinIntrinsicHeight(width);
            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        public override double computeMaxIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMaxIntrinsicHeight(width);
            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        public override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._additionalConstraints.enforce(this.constraints), parentUsesSize: true);
                this.size = this.child.size;
            } else {
                this.size = this._additionalConstraints.enforce(this.constraints).constrain(Size.zero);
            }
        }
    }

    public class RenderLimitedBox : RenderProxyBox {
        public RenderLimitedBox(
            RenderBox child = null,
            double maxWidth = double.PositiveInfinity,
            double maxHeight = double.PositiveInfinity
        ) : base(child) {
            this._maxWidth = maxWidth;
            this._maxHeight = maxHeight;
        }

        public double maxWidth {
            get { return this._maxWidth; }
            set {
                if (this._maxWidth == value) {
                    return;
                }

                this._maxWidth = value;
                this.markNeedsLayout();
            }
        }

        public double _maxWidth;

        public double maxHeight {
            get { return this._maxHeight; }
            set {
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        public double _maxHeight;

        public BoxConstraints _limitConstraints(BoxConstraints constraints) {
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

        public override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._limitConstraints(this.constraints), parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
            } else {
                this.size = this._limitConstraints(this.constraints).constrain(Size.zero);
            }
        }
    }

    public enum DecorationPosition {
        background,
        foreground,
    }

    public class RenderDecoratedBox : RenderProxyBox {
        public RenderDecoratedBox(
            Decoration decoration,
            DecorationPosition position = DecorationPosition.background,
            ImageConfiguration configuration = null,
            RenderBox child = null
        ) : base(child) {
            this._decoration = decoration;
            this._position = position;
            this._configuration = configuration ?? ImageConfiguration.empty;
        }

        public BoxPainter _painter;

        public Decoration decoration {
            get { return this._decoration; }
            set {
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

        public Decoration _decoration;

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

        public DecorationPosition _position;


        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.markNeedsPaint();
            }
        }

        public ImageConfiguration _configuration;

        public override void detach() {
            if (this._painter != null) {
                this._painter.dispose();
                this._painter = null;
            }

            base.detach();
            this.markNeedsPaint();
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._painter == null) {
                this._painter = this._decoration.createBoxPainter(this.markNeedsPaint);
            }

            var filledConfiguration = this.configuration.copyWith(size: this.size);
            if (this.position == DecorationPosition.background) {
                this._painter.paint(context.canvas, offset, filledConfiguration);
            }

            base.paint(context, offset);

            if (this.position == DecorationPosition.foreground) {
                this._painter.paint(context.canvas, offset, filledConfiguration);
            }
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

        public override void performResize() {
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

    public class RenderTransform : RenderProxyBox {
        public RenderTransform(
            Matrix4x4 transform,
            Offset origin,
            Alignment alignment,
            TextDirection textDirection,
            RenderBox child = null,
            bool transformHitTests = true
        ) {
            this.transform = transform;
            this.origin = origin;
            this.alignment = alignment;
            this.textDirection = textDirection;
            this.child = child;
            this.transformHitTests = transformHitTests;
        }

        public Offset origin {
            get { return _origin; }
            set {
                if (_origin.Equals(value)) {
                    return;
                }

                _origin = value;
                markNeedsPaint();
            }
        }

        private Offset _origin;

        public Alignment alignment {
            get { return _alignment; }
            set {
                if (_alignment.Equals(value)) {
                    return;
                }

                _alignment = value;
                markNeedsPaint();
            }
        }

        private Alignment _alignment;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection.Equals(value)) {
                    return;
                }

                _textDirection = value;
                markNeedsPaint();
            }
        }

        private TextDirection _textDirection;

        public bool transformHitTests;

        public Matrix4x4 transform {
            set {
                if (_transform.Equals(value)) {
                    return;
                }

                _transform = value;
            }
        }

        private Matrix4x4 _transform;
    }

    public class RenderOpacity : RenderProxyBox {
        public RenderOpacity(RenderBox child = null, double opacity = 1.0) : base(child) {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            this._opacity = opacity;
            this._alpha = _getAlphaFromOpacity(opacity);
        }

        public override bool alwaysNeedsCompositing {
            get { return base.alwaysNeedsCompositing; }
        }

        private int _alpha;

        private static int _getAlphaFromOpacity(double opacity) {
            return (opacity * 255).round();
        }

        public double opacity {
            get { return _opacity; }
            set {
                D.assert(value >= 0.0 && value <= 1.0);
                if (_opacity == value) {
                    return;
                }

                bool didNeedCompositing = alwaysNeedsCompositing;
                bool wasVisible = _alpha != 0;
                _opacity = value;
                _alpha = _getAlphaFromOpacity(_opacity);
                if (didNeedCompositing != alwaysNeedsCompositing) {
                    markNeedsCompositingBitsUpdate();
                }

                markNeedsPaint();
            }
        }

        private double _opacity;

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                if (_alpha == 0) {
                    return;
                }
            }

            if (_alpha == 255) {
                context.paintChild(child, offset);
                return;
            }

            D.assert(needsCompositing);
            context.pushOpacity(offset, _alpha, base.paint);
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