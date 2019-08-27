using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Gradient = Unity.UIWidgets.ui.Gradient;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.rendering {
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

        protected override float computeMinIntrinsicWidth(float height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.minWidth;
            }

            float width = base.computeMinIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.minWidth;
            }

            float width = base.computeMaxIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            float height = base.computeMinIntrinsicHeight(width);
            D.assert(height.isFinite());

            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            float height = base.computeMaxIntrinsicHeight(width);
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
            }
            else {
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
                    context.canvas.drawRect(offset & this.size, paint);
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
            float maxWidth = float.PositiveInfinity,
            float maxHeight = float.PositiveInfinity
        ) : base(child) {
            D.assert(maxWidth >= 0.0);
            D.assert(maxHeight >= 0.0);

            this._maxWidth = maxWidth;
            this._maxHeight = maxHeight;
        }

        public float maxWidth {
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

        float _maxWidth;

        public float maxHeight {
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

        float _maxHeight;

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
            }
            else {
                this.size = this._limitConstraints(this.constraints).constrain(Size.zero);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("maxWidth", this.maxWidth, defaultValue: float.PositiveInfinity));
            properties.add(new FloatProperty("maxHeight", this.maxHeight, defaultValue: float.PositiveInfinity));
        }
    }

    public class RenderAspectRatio : RenderProxyBox {
        public RenderAspectRatio(float aspectRatio, RenderBox child = null) : base(child) {
            this._aspectRatio = aspectRatio;
        }

        public float aspectRatio {
            get { return this._aspectRatio; }
            set {
                if (this._aspectRatio == value) {
                    return;
                }

                this._aspectRatio = value;
                this.markNeedsLayout();
            }
        }

        float _aspectRatio;


        protected override float computeMinIntrinsicWidth(float height) {
            if (height.isFinite()) {
                return height * this._aspectRatio;
            }

            if (this.child != null) {
                return this.child.getMinIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (height.isFinite()) {
                return height * this._aspectRatio;
            }

            if (this.child != null) {
                return this.child.getMaxIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (width.isFinite()) {
                return width / this._aspectRatio;
            }

            if (this.child != null) {
                return this.child.getMinIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (width.isFinite()) {
                return width / this._aspectRatio;
            }

            if (this.child != null) {
                return this.child.getMaxIntrinsicHeight(width);
            }

            return 0.0f;
        }

        Size _applyAspectRatio(BoxConstraints constraints) {
            D.assert(constraints.debugAssertIsValid());
            if (constraints.isTight) {
                return constraints.smallest;
            }

            float width = constraints.maxWidth;
            float height = width / this._aspectRatio;

            if (width > constraints.maxWidth) {
                width = constraints.maxWidth;
                height = width / this._aspectRatio;
            }

            if (height > constraints.maxHeight) {
                height = constraints.maxHeight;
                width = height * this._aspectRatio;
            }

            if (width < constraints.minWidth) {
                width = constraints.minWidth;
                height = width / this._aspectRatio;
            }

            if (height < constraints.minHeight) {
                height = constraints.minHeight;
                width = height * this._aspectRatio;
            }

            return constraints.constrain(new Size(width, height));
        }

        protected override void performLayout() {
            this.size = this._applyAspectRatio(this.constraints);
            if (this.child != null) {
                this.child.layout(BoxConstraints.tight(this.size));
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("aspectRatio", this.aspectRatio));
        }
    }

    public class RenderIntrinsicWidth : RenderProxyBox {
        public RenderIntrinsicWidth(
            float? stepWidth = null,
            float? stepHeight = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(stepWidth == null || stepWidth > 0.0f);
            D.assert(stepHeight == null || stepHeight > 0.0f);
            this._stepWidth = stepWidth;
            this._stepHeight = stepHeight;
        }

        float? _stepWidth;

        public float? stepWidth {
            get { return this._stepWidth; }
            set {
                D.assert(value == null || value > 0.0f);
                if (value == this._stepWidth) {
                    return;
                }

                this._stepWidth = value;
                this.markNeedsLayout();
            }
        }

        float? _stepHeight;

        public float? stepHeight {
            get { return this._stepHeight; }
            set {
                D.assert(value == null || value > 0.0f);
                if (value == this._stepHeight) {
                    return;
                }

                this._stepHeight = value;
                this.markNeedsLayout();
            }
        }

        static float _applyStep(float input, float? step) {
            D.assert(input.isFinite());
            if (step == null) {
                return input;
            }

            return (input / step.Value).ceil() * step.Value;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this.computeMaxIntrinsicWidth(height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.child == null) {
                return 0.0f;
            }

            float width = this.child.getMaxIntrinsicWidth(height);
            return _applyStep(width, this._stepWidth);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.child == null) {
                return 0.0f;
            }

            if (!width.isFinite()) {
                width = this.computeMaxIntrinsicWidth(float.PositiveInfinity);
            }

            D.assert(width.isFinite());
            float height = this.child.getMinIntrinsicHeight(width);
            return _applyStep(height, this._stepHeight);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.child == null) {
                return 0.0f;
            }

            if (!width.isFinite()) {
                width = this.computeMaxIntrinsicWidth(float.PositiveInfinity);
            }

            D.assert(width.isFinite());
            float height = this.child.getMaxIntrinsicHeight(width);
            return _applyStep(height, this._stepHeight);
        }

        protected override void performLayout() {
            if (this.child != null) {
                BoxConstraints childConstraints = this.constraints;
                if (!childConstraints.hasTightWidth) {
                    float width = this.child.getMaxIntrinsicWidth(childConstraints.maxHeight);
                    D.assert(width.isFinite());
                    childConstraints = childConstraints.tighten(width: _applyStep(width, this._stepWidth));
                }

                if (this._stepHeight != null) {
                    float height = this.child.getMaxIntrinsicHeight(childConstraints.maxWidth);
                    D.assert(height.isFinite());
                    childConstraints = childConstraints.tighten(height: _applyStep(height, this._stepHeight));
                }

                this.child.layout(childConstraints, parentUsesSize: true);
                this.size = this.child.size;
            }
            else {
                this.performResize();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("stepWidth", this.stepWidth));
            properties.add(new FloatProperty("stepHeight", this.stepHeight));
        }
    }

    public class RenderIntrinsicHeight : RenderProxyBox {
        public RenderIntrinsicHeight(
            RenderBox child = null
        ) : base(child) {
        }

        protected override float computeMinIntrinsicWidth(float height) {
            if (this.child == null) {
                return 0.0f;
            }

            if (!height.isFinite()) {
                height = this.child.getMaxIntrinsicHeight(float.PositiveInfinity);
            }

            D.assert(height.isFinite());
            return this.child.getMinIntrinsicWidth(height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.child == null) {
                return 0.0f;
            }

            if (!height.isFinite()) {
                height = this.child.getMaxIntrinsicHeight(float.PositiveInfinity);
            }

            D.assert(height.isFinite());
            return this.child.getMaxIntrinsicWidth(height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this.computeMaxIntrinsicHeight(width);
        }

        protected override void performLayout() {
            if (this.child != null) {
                BoxConstraints childConstraints = this.constraints;
                if (!childConstraints.hasTightHeight) {
                    float height = this.child.getMaxIntrinsicHeight(childConstraints.maxWidth);
                    D.assert(height.isFinite());
                    childConstraints = childConstraints.tighten(height: height);
                }

                this.child.layout(childConstraints, parentUsesSize: true);
                this.size = this.child.size;
            }
            else {
                this.performResize();
            }
        }
    }

    public class RenderOpacity : RenderProxyBox {
        public RenderOpacity(float opacity = 1.0f, RenderBox child = null) : base(child) {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            this._opacity = opacity;
            this._alpha = _getAlphaFromOpacity(opacity);
        }

        protected override bool alwaysNeedsCompositing {
            get { return this.child != null && (this._alpha != 0 && this._alpha != 255); }
        }

        int _alpha;

        internal static int _getAlphaFromOpacity(float opacity) {
            return (opacity * 255).round();
        }

        float _opacity;

        public float opacity {
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
            properties.add(new FloatProperty("opacity", this.opacity));
        }
    }

    public class RenderAnimatedOpacity : RenderProxyBox {
        public RenderAnimatedOpacity(
            Animation<float> opacity = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(opacity != null);
            this.opacity = opacity;
        }


        int _alpha;

        protected override bool alwaysNeedsCompositing {
            get { return this.child != null && this._currentlyNeedsCompositing; }
        }

        bool _currentlyNeedsCompositing;

        Animation<float> _opacity;

        public Animation<float> opacity {
            get { return this._opacity; }
            set {
                D.assert(value != null);
                if (this._opacity == value) {
                    return;
                }

                if (this.attached && this._opacity != null) {
                    this._opacity.removeListener(this._updateOpacity);
                }

                this._opacity = value;
                if (this.attached) {
                    this._opacity.addListener(this._updateOpacity);
                }

                this._updateOpacity();
            }
        }


        public override void attach(object owner) {
            base.attach(owner);
            this._opacity.addListener(this._updateOpacity);
            this._updateOpacity(); // in case it changed while we weren't listening
        }

        public override void detach() {
            this._opacity.removeListener(this._updateOpacity);
            base.detach();
        }

        public void _updateOpacity() {
            var oldAlpha = this._alpha;
            this._alpha = RenderOpacity._getAlphaFromOpacity(this._opacity.value.clamp(0.0f, 1.0f));
            if (oldAlpha != this._alpha) {
                bool didNeedCompositing = this._currentlyNeedsCompositing;
                this._currentlyNeedsCompositing = this._alpha > 0 && this._alpha < 255;
                if (this.child != null && didNeedCompositing != this._currentlyNeedsCompositing) {
                    this.markNeedsCompositingBitsUpdate();
                }

                this.markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                if (this._alpha == 0) {
                    return;
                }

                if (this._alpha == 255) {
                    context.paintChild(this.child, offset);
                    return;
                }

                D.assert(this.needsCompositing);
                context.pushOpacity(offset, this._alpha, base.paint);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", this.opacity));
        }
    }

    public class RenderBackdropFilter : RenderProxyBox {
        public RenderBackdropFilter(
            RenderBox child = null,
            ImageFilter filter = null
        ) : base(child) {
            D.assert(filter != null);
            this._filter = filter;
        }

        ImageFilter _filter;

        public ImageFilter filter {
            get { return this._filter; }
            set {
                D.assert(value != null);
                if (this._filter == value) {
                    return;
                }

                this.markNeedsPaint();
            }
        }

        protected override bool alwaysNeedsCompositing {
            get { return this.child != null; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                D.assert(this.needsCompositing);
                context.pushLayer(new BackdropFilterLayer(this.filter), base.paint, offset);
            }
        }
    }

    public abstract class CustomClipper<T> {
        public CustomClipper(Listenable reclip = null) {
            this._reclip = reclip;
        }

        public readonly Listenable _reclip;

        public abstract T getClip(Size size);

        public virtual Rect getApproximateClipRect(Size size) {
            return Offset.zero & size;
        }

        public abstract bool shouldReclip(CustomClipper<T> oldClipper);

        public override string ToString() {
            return this.GetType() + "";
        }
    }

    public class ShapeBorderClipper : CustomClipper<Path> {
        public ShapeBorderClipper(
            ShapeBorder shape = null) {
            D.assert(shape != null);
            this.shape = shape;
        }

        public readonly ShapeBorder shape;

        public override Path getClip(Size size) {
            return this.shape.getOuterPath(Offset.zero & size);
        }

        public override bool shouldReclip(CustomClipper<Path> oldClipper) {
            if (oldClipper.GetType() != this.GetType()) {
                return true;
            }

            ShapeBorderClipper typedOldClipper = (ShapeBorderClipper) oldClipper;
            return typedOldClipper.shape != this.shape;
        }
    }

    public abstract class _RenderCustomClip<T> : RenderProxyBox where T : class {
        protected _RenderCustomClip(
            RenderBox child = null,
            CustomClipper<T> clipper = null,
            Clip clipBehavior = Clip.antiAlias) : base(child: child) {
            this.clipBehavior = clipBehavior;
            this._clipper = clipper;
        }

        public CustomClipper<T> clipper {
            get { return this._clipper; }
            set {
                if (this._clipper == value) {
                    return;
                }

                CustomClipper<T> oldClipper = this._clipper;
                this._clipper = value;
                D.assert(value != null || oldClipper != null);
                if (value == null || oldClipper == null ||
                    value.GetType() != oldClipper.GetType() ||
                    value.shouldReclip(oldClipper)) {
                    this._markNeedsClip();
                }

                if (this.attached) {
                    oldClipper?._reclip?.removeListener(this._markNeedsClip);
                    value?._reclip?.addListener(this._markNeedsClip);
                }
            }
        }

        protected CustomClipper<T> _clipper;

        public override void attach(object owner) {
            base.attach(owner);
            this._clipper?._reclip?.addListener(this._markNeedsClip);
        }

        public override void detach() {
            this._clipper?._reclip?.removeListener(this._markNeedsClip);
            base.detach();
        }

        protected void _markNeedsClip() {
            this._clip = null;
            this.markNeedsPaint();
        }

        protected abstract T _defaultClip { get; }
        protected T _clip;

        public readonly Clip clipBehavior;

        protected override void performLayout() {
            Size oldSize = this.hasSize ? this.size : null;
            base.performLayout();
            if (oldSize != this.size) {
                this._clip = null;
            }
        }

        protected void _updateClip() {
            this._clip = this._clip ?? this._clipper?.getClip(this.size) ?? this._defaultClip;
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            return this._clipper?.getApproximateClipRect(this.size) ?? Offset.zero & this.size;
        }

        protected Paint _debugPaint;
        protected TextPainter _debugText;

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this._debugPaint == null) {
                    this._debugPaint = new Paint();
                    this._debugPaint.shader = Gradient.linear(
                        new Offset(0.0f, 0.0f),
                        new Offset(10.0f, 10.0f),
                        new List<Color> {
                            new Color(0x00000000),
                            new Color(0xFFFF00FF),
                        }, null, TileMode.repeated);
                    this._debugPaint.strokeWidth = 2.0f;
                    this._debugPaint.style = PaintingStyle.stroke;
                }

                if (this._debugText == null) {
                    this._debugText = new TextPainter(
                        text: new TextSpan(
                            text: "x",
                            style: new TextStyle(
                                color: new Color(0xFFFF00FF),
                                fontSize: 14.0f)
                        ));
                    this._debugText.layout();
                }

                return true;
            });
        }
    }

    public class RenderClipRect : _RenderCustomClip<Rect> {
        public RenderClipRect(
            RenderBox child = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(
            child: child,
            clipper: clipper,
            clipBehavior: clipBehavior) {
        }

        protected override Rect _defaultClip {
            get { return Offset.zero & this.size; }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (this._clipper != null) {
                this._updateClip();
                D.assert(this._clip != null);
                if (!this._clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                context.pushClipRect(this.needsCompositing, offset, this._clip,
                    base.paint, clipBehavior: this.clipBehavior);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this.child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawRect(this._clip.shift(offset), this._debugPaint);
                    this._debugText.paint(context.canvas,
                        offset + new Offset(this._clip.width / 8.0f,
                            -(this._debugText.text.style.fontSize ?? 0.0f) * 1.1f));
                }

                return true;
            });
        }
    }


    public class RenderClipRRect : _RenderCustomClip<RRect> {
        public RenderClipRRect(
            RenderBox child = null,
            BorderRadius borderRadius = null,
            CustomClipper<RRect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
            this._borderRadius = borderRadius ?? BorderRadius.zero;
            D.assert(this._borderRadius != null || clipper != null);
        }

        public BorderRadius borderRadius {
            get { return this._borderRadius; }
            set {
                D.assert(value != null);
                if (this._borderRadius == value) {
                    return;
                }

                this._borderRadius = value;
                this._markNeedsClip();
            }
        }

        BorderRadius _borderRadius;

        protected override RRect _defaultClip {
            get { return this._borderRadius.toRRect(Offset.zero & this.size); }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (this._clipper != null) {
                this._updateClip();
                D.assert(this._clip != null);
                if (!this._clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                context.pushClipRRect(this.needsCompositing, offset, this._clip.outerRect, this._clip,
                    base.paint, clipBehavior: this.clipBehavior);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this.child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawRRect(this._clip.shift(offset), this._debugPaint);
                    this._debugText.paint(context.canvas,
                        offset + new Offset(this._clip.tlRadiusX,
                            -(this._debugText.text.style.fontSize ?? 0.0f) * 1.1f));
                }

                return true;
            });
        }
    }

    public class RenderClipOval : _RenderCustomClip<Rect> {
        public RenderClipOval(
            RenderBox child = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
        }

        Rect _cachedRect;
        Path _cachedPath;

        Path _getClipPath(Rect rect) {
            if (rect != this._cachedRect) {
                this._cachedRect = rect;
                this._cachedPath = new Path();
                this._cachedPath.addOval(this._cachedRect);
            }

            return this._cachedPath;
        }

        protected override Rect _defaultClip {
            get { return Offset.zero & this.size; }
        }

        public override bool hitTest(HitTestResult result,
            Offset position = null
        ) {
            this._updateClip();
            D.assert(this._clip != null);
            Offset center = this._clip.center;
            Offset offset = new Offset((position.dx - center.dx) / this._clip.width,
                (position.dy - center.dy) / this._clip.height);
            if (offset.distanceSquared > 0.25f) {
                return false;
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                context.pushClipPath(this.needsCompositing, offset, this._clip, this._getClipPath(this._clip),
                    base.paint, clipBehavior: this.clipBehavior);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this.child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawPath(this._getClipPath(this._clip).shift(offset), this._debugPaint);
                    this._debugText.paint(context.canvas,
                        offset + new Offset((this._clip.width - this._debugText.width) / 2.0f,
                            -this._debugText.text.style.fontSize * 1.1f ?? 0.0f));
                }

                return true;
            });
        }
    }

    public class RenderClipPath : _RenderCustomClip<Path> {
        public RenderClipPath(
            RenderBox child = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
        }

        protected override Path _defaultClip {
            get {
                var path = new Path();
                path.addRect(Offset.zero & this.size);
                return path;
            }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            D.assert(position != null);

            if (this._clipper != null) {
                this._updateClip();
                D.assert(this._clip != null);
                if (!this._clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                context.pushClipPath(this.needsCompositing, offset, Offset.zero & this.size,
                    this._clip, base.paint, clipBehavior: this.clipBehavior);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this.child != null) {
                    base.debugPaintSize(context, offset);
                    Path offsetPath = new Path();
                    offsetPath.addPath(this._clip, offset);
                    context.canvas.drawPath(offsetPath, this._debugPaint);
                    this._debugText.paint(context.canvas, offset);
                }

                return true;
            });
        }
    }

    public abstract class _RenderPhysicalModelBase<T> : _RenderCustomClip<T> where T : class {
        public _RenderPhysicalModelBase(
            RenderBox child = null,
            float? elevation = null,
            Color color = null,
            Color shadowColor = null,
            Clip clipBehavior = Clip.none,
            CustomClipper<T> clipper = null
        ) : base(child: child, clipBehavior: clipBehavior, clipper: clipper) {
            D.assert(elevation != null && elevation >= 0.0f);
            D.assert(color != null);
            D.assert(shadowColor != null);
            this._elevation = elevation ?? 0.0f;
            this._color = color;
            this._shadowColor = shadowColor;
        }

        public float elevation {
            get { return this._elevation; }
            set {
                D.assert(value >= 0.0f);
                if (this.elevation == value) {
                    return;
                }

                bool didNeedCompositing = this.alwaysNeedsCompositing;
                this._elevation = value;
                if (didNeedCompositing != this.alwaysNeedsCompositing) {
                    this.markNeedsCompositingBitsUpdate();
                }

                this.markNeedsPaint();
            }
        }

        float _elevation;

        public Color shadowColor {
            get { return this._shadowColor; }
            set {
                D.assert(value != null);
                if (this.shadowColor == value) {
                    return;
                }

                this._shadowColor = value;
                this.markNeedsPaint();
            }
        }

        Color _shadowColor;

        public Color color {
            get { return this._color; }
            set {
                D.assert(value != null);
                if (this.color == value) {
                    return;
                }

                this._color = value;
                this.markNeedsPaint();
            }
        }

        Color _color;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FloatProperty("elevation", this.elevation));
            description.add(new DiagnosticsProperty<Color>("color", this.color));
            description.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
        }
    }

    public class RenderPhysicalModel : _RenderPhysicalModelBase<RRect> {
        public RenderPhysicalModel(
            RenderBox child = null,
            BoxShape shape = BoxShape.rectangle,
            Clip clipBehavior = Clip.none,
            BorderRadius borderRadius = null,
            float? elevation = 0.0f,
            Color color = null,
            Color shadowColor = null
        ) : base(clipBehavior: clipBehavior, child: child, elevation: elevation, color: color,
            shadowColor: shadowColor ?? new Color(0xFF000000)) {
            D.assert(color != null);
            D.assert(elevation != null && elevation >= 0.0f);
            this._shape = shape;
            this._borderRadius = borderRadius;
        }

        public BoxShape shape {
            get { return this._shape; }
            set {
                if (this.shape == value) {
                    return;
                }

                this._shape = value;
                this._markNeedsClip();
            }
        }

        BoxShape _shape;

        public BorderRadius borderRadius {
            get { return this._borderRadius; }
            set {
                if (this.borderRadius == value) {
                    return;
                }

                this._borderRadius = value;
                this._markNeedsClip();
            }
        }

        BorderRadius _borderRadius;

        protected override RRect _defaultClip {
            get {
                D.assert(this.hasSize);
                switch (this._shape) {
                    case BoxShape.rectangle:
                        return (this.borderRadius ?? BorderRadius.zero).toRRect(Offset.zero & this.size);
                    case BoxShape.circle:
                        Rect rect = Offset.zero & this.size;
                        return RRect.fromRectXY(rect, rect.width / 2, rect.height / 2);
                }

                return null;
            }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (this._clipper != null) {
                this._updateClip();
                D.assert(this._clip != null);
                if (!this._clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                RRect offsetRRect = this._clip.shift(offset);
                Rect offsetBounds = offsetRRect.outerRect;
                Path offsetRRectAsPath = new Path();
                offsetRRectAsPath.addRRect(offsetRRect);

                PhysicalModelLayer physicalModel = new PhysicalModelLayer(
                    clipPath: offsetRRectAsPath,
                    clipBehavior: this.clipBehavior,
                    elevation: this.elevation,
                    color: this.color,
                    shadowColor: this.shadowColor);
                D.assert(() => {
                    physicalModel.debugCreator = this.debugCreator;
                    return true;
                });
                context.pushLayer(physicalModel, base.paint, offset, childPaintBounds: offsetBounds);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<BoxShape>("shape", this.shape));
            description.add(new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius));
        }
    }


    public class RenderPhysicalShape : _RenderPhysicalModelBase<Path> {
        public RenderPhysicalShape(
            RenderBox child = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.none,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null
        ) : base(child: child,
            elevation: elevation,
            color: color,
            shadowColor: shadowColor ?? new Color(0xFF000000),
            clipper: clipper,
            clipBehavior: clipBehavior) {
            D.assert(clipper != null);
            D.assert(color != null);
        }

        protected override Path _defaultClip {
            get {
                Path path = new Path();
                path.addRect(Offset.zero & this.size);
                return path;
            }
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (this._clipper != null) {
                this._updateClip();
                D.assert(this._clip != null);
                if (!this._clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                this._updateClip();
                Rect offsetBounds = offset & this.size;
                Path offsetPath = new Path();
                offsetPath.addPath(this._clip, offset);

                PhysicalModelLayer physicalModel = new PhysicalModelLayer(
                    clipPath: offsetPath,
                    clipBehavior: this.clipBehavior,
                    elevation: this.elevation,
                    color: this.color,
                    shadowColor: this.shadowColor);
                context.pushLayer(physicalModel, base.paint, offset, childPaintBounds: offsetBounds);
            }
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", this.clipper));
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
                    this._painter.Dispose();
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
                this._painter.Dispose();
                this._painter = null;
            }

            base.detach();
            this.markNeedsPaint();
        }

        protected override bool hitTestSelf(Offset position) {
            return this._decoration.hitTest(this.size, position);
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
            Matrix3 transform,
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

        public Matrix3 transform {
            set {
                if (this._transform == value) {
                    return;
                }

                this._transform = value;
                this.markNeedsPaint();
            }
        }

        Matrix3 _transform;

        public void setIdentity() {
            this._transform = Matrix3.I();
            this.markNeedsPaint();
        }

        public void rotateX(float degrees) {
            //2D, do nothing
        }

        public void rotateY(float degrees) {
            //2D, do nothing
        }

        public void rotateZ(float degrees) {
            this._transform.preRotate(degrees);
            this.markNeedsPaint();
        }

        public void translate(float x, float y = 0.0f, float z = 0.0f) {
            this._transform.preTranslate(x, y);
            this.markNeedsPaint();
        }

        public void scale(float x, float y, float z) {
            this._transform.preScale(x, y);
            this.markNeedsPaint();
        }

        Matrix3 _effectiveTransform {
            get {
                Alignment resolvedAlignment = this.alignment;
                if (this._origin == null && resolvedAlignment == null) {
                    return this._transform;
                }

                var result = Matrix3.I();
                if (this._origin != null) {
                    result.preTranslate(this._origin.dx, this._origin.dy);
                }

                Offset translation = null;
                if (resolvedAlignment != null) {
                    translation = resolvedAlignment.alongSize(this.size);
                    result.preTranslate(translation.dx, translation.dy);
                }

                result.preConcat(this._transform);

                if (resolvedAlignment != null) {
                    result.preTranslate(-translation.dx, -translation.dy);
                }

                if (this._origin != null) {
                    result.preTranslate(-this._origin.dx, -this._origin.dy);
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
                var inverse = Matrix3.I();
                var invertible = transform.invert(inverse);

                if (!invertible) {
                    return false;
                }

                position = inverse.mapPoint(position);
            }

            return base.hitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                var transform = this._effectiveTransform;
                Offset childOffset = transform.getAsTranslation();

                if (childOffset == null) {
                    context.pushTransform(this.needsCompositing, offset, transform, base.paint);
                }
                else {
                    base.paint(context, offset + childOffset);
                }
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            transform.preConcat(this._effectiveTransform);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Matrix3>("transform matrix", this._transform));
            properties.add(new DiagnosticsProperty<Offset>("origin", this.origin));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
            properties.add(new DiagnosticsProperty<bool>("transformHitTests", this.transformHitTests));
        }
    }

    public class RenderFittedBox : RenderProxyBox {
        public RenderFittedBox(
            BoxFit fit = BoxFit.contain,
            Alignment alignment = null,
            RenderBox child = null
        ) : base(child) {
            this._fit = fit;
            this._alignment = alignment ?? Alignment.center;
        }

        Alignment _resolvedAlignment;

        void _resolve() {
            if (this._resolvedAlignment != null) {
                return;
            }

            this._resolvedAlignment = this.alignment;
        }

        void _markNeedResolution() {
            this._resolvedAlignment = null;
            this.markNeedsPaint();
        }

        public BoxFit fit {
            get { return this._fit; }
            set {
                if (this._fit == value) {
                    return;
                }

                this._fit = value;
                this._clearPaintData();
                this.markNeedsPaint();
            }
        }

        BoxFit _fit;

        public Alignment alignment {
            get { return this._alignment; }
            set {
                D.assert(value != null);
                if (this._alignment == value) {
                    return;
                }

                this._alignment = value;
                this._clearPaintData();
                this._markNeedResolution();
            }
        }

        Alignment _alignment;

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(new BoxConstraints(), parentUsesSize: true);
                this.size = this.constraints.constrainSizeAndAttemptToPreserveAspectRatio(this.child.size);
                this._clearPaintData();
            }
            else {
                this.size = this.constraints.smallest;
            }
        }

        bool? _hasVisualOverflow;
        Matrix3 _transform;

        void _clearPaintData() {
            this._hasVisualOverflow = null;
            this._transform = null;
        }

        void _updatePaintData() {
            if (this._transform != null) {
                return;
            }

            if (this.child == null) {
                this._hasVisualOverflow = false;
                this._transform = Matrix3.I();
            }
            else {
                this._resolve();
                Size childSize = this.child.size;
                FittedSizes sizes = FittedSizes.applyBoxFit(this._fit, childSize, this.size);
                float scaleX = sizes.destination.width / sizes.source.width;
                float scaleY = sizes.destination.height / sizes.source.height;
                Rect sourceRect = this._resolvedAlignment.inscribe(sizes.source, Offset.zero & childSize);
                Rect destinationRect = this._resolvedAlignment.inscribe(sizes.destination, Offset.zero & this.size);
                this._hasVisualOverflow = sourceRect.width < childSize.width || sourceRect.height < childSize.height;
                this._transform = Matrix3.makeTrans(destinationRect.left, destinationRect.top);
                this._transform.postScale(scaleX, scaleY);
                this._transform.postTranslate(-sourceRect.left, -sourceRect.top);
            }
        }

        void _paintChildWithTransform(PaintingContext context, Offset offset) {
            Offset childOffset = this._transform.getAsTranslation();
            if (childOffset == null) {
                context.pushTransform(this.needsCompositing, offset, this._transform, base.paint);
            }
            else {
                base.paint(context, offset + childOffset);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.size.isEmpty) {
                return;
            }

            this._updatePaintData();
            if (this.child != null) {
                if (this._hasVisualOverflow == true) {
                    context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size,
                        this._paintChildWithTransform);
                }
                else {
                    this._paintChildWithTransform(context, offset);
                }
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            if (this.size.isEmpty) {
                return false;
            }

            this._updatePaintData();
            Matrix3 inverse = Matrix3.I();
            if (!this._transform.invert(inverse)) {
                return false;
            }

            position = inverse.mapPoint(position);
            return base.hitTestChildren(result, position: position);
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            if (this.size.isEmpty) {
                transform.setAll(0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            else {
                this._updatePaintData();
                transform.postConcat(this._transform);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxFit>("fit", this.fit));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
        }
    }

    public class RenderFractionalTranslation : RenderProxyBox {
        public RenderFractionalTranslation(
            Offset translation = null,
            bool transformHitTests = true,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(translation != null);
            this._translation = translation;
            this.transformHitTests = transformHitTests;
        }

        public Offset translation {
            get { return this._translation; }

            set {
                D.assert(value != null);
                if (this._translation == value) {
                    return;
                }

                this._translation = value;
                this.markNeedsPaint();
            }
        }

        Offset _translation;

        public override bool hitTest(HitTestResult result, Offset position) {
            return this.hitTestChildren(result, position: position);
        }

        public bool transformHitTests;

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            D.assert(!this.debugNeedsLayout);
            if (this.transformHitTests) {
                position = new Offset(
                    position.dx - this.translation.dx * this.size.width,
                    position.dy - this.translation.dy * this.size.height
                );
            }

            return base.hitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(!this.debugNeedsLayout);
            if (this.child != null) {
                base.paint(context, new Offset(
                    offset.dx + this.translation.dx * this.size.width,
                    offset.dy + this.translation.dy * this.size.height
                ));
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            transform.preTranslate(this.translation.dx * this.size.width,
                this.translation.dy * this.size.height);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("translation", this.translation));
            properties.add(new DiagnosticsProperty<bool>("transformHitTests", this.transformHitTests));
        }
    }

    public delegate void PointerDownEventListener(PointerDownEvent evt);

    public delegate void PointerMoveEventListener(PointerMoveEvent evt);

    public delegate void PointerUpEventListener(PointerUpEvent evt);

    public delegate void PointerCancelEventListener(PointerCancelEvent evt);

    public delegate void PointerSignalEventListener(PointerSignalEvent evt);

    public delegate void PointerScrollEventListener(PointerScrollEvent evt);

    public class RenderPointerListener : RenderProxyBoxWithHitTestBehavior {
        public RenderPointerListener(
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerEnterEventListener onPointerEnter = null,
            PointerHoverEventListener onPointerHover = null,
            PointerExitEventListener onPointerExit = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            PointerSignalEventListener onPointerSignal = null,
            PointerScrollEventListener onPointerScroll = null,
            PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter = null,
            PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover = null,
            PointerDragFromEditorExitEventListener onPointerDragFromEditorExit = null,
            PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(behavior: behavior, child: child) {
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.onPointerSignal = onPointerSignal;
            this.onPointerScroll = onPointerScroll;

            this._onPointerEnter = onPointerEnter;
            this._onPointerHover = onPointerHover;
            this._onPointerExit = onPointerExit;

            this._onPointerDragFromEditorEnter = onPointerDragFromEditorEnter;
            this._onPointerDragFromEditorHover = onPointerDragFromEditorHover;
            this._onPointerDragFromEditorExit = onPointerDragFromEditorExit;
            this._onPointerDragFromEditorRelease = onPointerDragFromEditorRelease;

            if (this._onPointerEnter != null ||
                this._onPointerHover != null ||
                this._onPointerExit != null ||
                this._onPointerDragFromEditorEnter != null ||
                this._onPointerDragFromEditorHover != null ||
                this._onPointerDragFromEditorExit != null ||
                this._onPointerDragFromEditorRelease != null
            ) {
                this._hoverAnnotation = new MouseTrackerAnnotation(
                    onEnter: this._onPointerEnter,
                    onHover: this._onPointerHover,
                    onExit: this._onPointerExit,
                    onDragFromEditorEnter: this._onPointerDragFromEditorEnter,
                    onDragFromEditorHover: this._onPointerDragFromEditorHover,
                    onDragFromEditorExit: this._onPointerDragFromEditorExit,
                    onDragFromEditorRelease: this._onPointerDragFromEditorRelease
                );
            }
        }

        PointerDragFromEditorEnterEventListener _onPointerDragFromEditorEnter;

        public PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter {
            get { return this._onPointerDragFromEditorEnter; }
            set {
                if (this._onPointerDragFromEditorEnter != value) {
                    this._onPointerDragFromEditorEnter = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerDragFromEditorExitEventListener _onPointerDragFromEditorExit;

        public PointerDragFromEditorExitEventListener onPointerDragFromEditorExit {
            get { return this._onPointerDragFromEditorExit; }
            set {
                if (this._onPointerDragFromEditorExit != value) {
                    this._onPointerDragFromEditorExit = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerDragFromEditorHoverEventListener _onPointerDragFromEditorHover;

        public PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover {
            get { return this._onPointerDragFromEditorHover; }
            set {
                if (this._onPointerDragFromEditorHover != value) {
                    this._onPointerDragFromEditorHover = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerDragFromEditorReleaseEventListener _onPointerDragFromEditorRelease;

        public PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease {
            get { return this._onPointerDragFromEditorRelease; }
            set {
                if (this._onPointerDragFromEditorRelease != value) {
                    this._onPointerDragFromEditorRelease = value;
                    this._updateAnnotations();
                }
            }
        }

        public PointerEnterEventListener onPointerEnter {
            get { return this._onPointerEnter; }
            set {
                if (this._onPointerEnter != value) {
                    this._onPointerEnter = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerEnterEventListener _onPointerEnter;

        public PointerHoverEventListener onPointerHover {
            get { return this._onPointerHover; }
            set {
                if (this._onPointerHover != value) {
                    this._onPointerHover = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerHoverEventListener _onPointerHover;

        public PointerExitEventListener onPointerExit {
            get { return this._onPointerExit; }
            set {
                if (this._onPointerExit != value) {
                    this._onPointerExit = value;
                    this._updateAnnotations();
                }
            }
        }

        PointerExitEventListener _onPointerExit;

        public PointerDownEventListener onPointerDown;

        public PointerMoveEventListener onPointerMove;

        public PointerUpEventListener onPointerUp;

        public PointerCancelEventListener onPointerCancel;

        public PointerSignalEventListener onPointerSignal;

        public PointerScrollEventListener onPointerScroll;

        MouseTrackerAnnotation _hoverAnnotation;

        public MouseTrackerAnnotation hoverAnnotation {
            get { return this._hoverAnnotation; }
        }

        void _updateAnnotations() {
            D.assert(this._onPointerEnter != this._hoverAnnotation.onEnter ||
                     this._onPointerHover != this._hoverAnnotation.onHover ||
                     this._onPointerExit != this._hoverAnnotation.onExit
#if UNITY_EDITOR
                     || this._onPointerDragFromEditorEnter != this._hoverAnnotation.onDragFromEditorEnter
                     || this._onPointerDragFromEditorHover != this._hoverAnnotation.onDragFromEditorHover
                     || this._onPointerDragFromEditorExit != this._hoverAnnotation.onDragFromEditorExit
                     || this._onPointerDragFromEditorRelease != this._hoverAnnotation.onDragFromEditorRelease
#endif
                , () => "Shouldn't call _updateAnnotations if nothing has changed.");

            if (this._hoverAnnotation != null && this.attached) {
                RendererBinding.instance.mouseTracker.detachAnnotation(this._hoverAnnotation);
            }

            if (this._onPointerEnter != null ||
                this._onPointerHover != null ||
                this._onPointerExit != null
#if UNITY_EDITOR
                || this._onPointerDragFromEditorEnter != null
                || this._onPointerDragFromEditorHover != null
                || this._onPointerDragFromEditorExit != null
                || this._onPointerDragFromEditorRelease != null
#endif
            ) {
                this._hoverAnnotation = new MouseTrackerAnnotation(
                    onEnter: this._onPointerEnter,
                    onHover: this._onPointerHover,
                    onExit: this._onPointerExit
#if UNITY_EDITOR
                    , onDragFromEditorEnter: this._onPointerDragFromEditorEnter
                    , onDragFromEditorHover: this._onPointerDragFromEditorHover
                    , onDragFromEditorExit: this._onPointerDragFromEditorExit
                    , onDragFromEditorRelease: this._onPointerDragFromEditorRelease
#endif
                );

                if (this.attached) {
                    RendererBinding.instance.mouseTracker.attachAnnotation(this._hoverAnnotation);
                }
            }
            else {
                this._hoverAnnotation = null;
            }

            this.markNeedsPaint();
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (this._hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.attachAnnotation(this._hoverAnnotation);
            }
        }

        public override void detach() {
            base.detach();
            if (this._hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.detachAnnotation(this._hoverAnnotation);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._hoverAnnotation != null) {
                AnnotatedRegionLayer<MouseTrackerAnnotation> layer = new AnnotatedRegionLayer<MouseTrackerAnnotation>(
                    this._hoverAnnotation, size: this.size, offset: offset);

                context.pushLayer(layer, base.paint, offset);
            }
            else {
                base.paint(context, offset);
            }
        }

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

            if (this.onPointerSignal != null && evt is PointerSignalEvent) {
                this.onPointerSignal((PointerSignalEvent) evt);
                return;
            }
            
            if (this.onPointerScroll != null && evt is PointerScrollEvent) {
                this.onPointerScroll((PointerScrollEvent) evt);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            var listeners = new List<string>();
            if (this.onPointerDown != null) {
                listeners.Add("down");
            }

            if (this.onPointerMove != null) {
                listeners.Add("move");
            }

            if (this.onPointerEnter != null) {
                listeners.Add("enter");
            }

            if (this.onPointerHover != null) {
                listeners.Add("hover");
            }

            if (this.onPointerExit != null) {
                listeners.Add("exit");
            }

            if (this.onPointerUp != null) {
                listeners.Add("up");
            }

            if (this.onPointerCancel != null) {
                listeners.Add("cancel");
            }

            if (this.onPointerSignal != null) {
                listeners.Add("signal");
            }

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
                }
                else {
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
                }
                else {
                    float fraction = (float) this.debugAsymmetricPaintCount /
                                     (this.debugSymmetricPaintCount + this.debugAsymmetricPaintCount);

                    string diagnosis;
                    if (this.debugSymmetricPaintCount + this.debugAsymmetricPaintCount < 5) {
                        diagnosis = "insufficient data to draw conclusion (less than five repaints)";
                    }
                    else if (fraction > 0.9) {
                        diagnosis = "this is an outstandingly useful repaint boundary and should definitely be kept";
                    }
                    else if (fraction > 0.5) {
                        diagnosis = "this is a useful repaint boundary and should be kept";
                    }
                    else if (fraction > 0.30) {
                        diagnosis =
                            "this repaint boundary is probably useful, but maybe it would be more useful in tandem with adding more repaint boundaries elsewhere";
                    }
                    else if (fraction > 0.1) {
                        diagnosis = "this repaint boundary does sometimes show value, though currently not that often";
                    }
                    else if (this.debugAsymmetricPaintCount == 0) {
                        diagnosis = "this repaint boundary is astoundingly ineffectual and should be removed";
                    }
                    else {
                        diagnosis = "this repaint boundary is not very effective and should probably be removed";
                    }

                    properties.add(new PercentProperty("metrics", fraction, unit: "useful",
                        tooltip: this.debugSymmetricPaintCount + " bad vs " + this.debugAsymmetricPaintCount +
                                 " good"));
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

    public class RenderOffstage : RenderProxyBox {
        public RenderOffstage(bool offstage = true,
            RenderBox child = null) : base(child) {
            this._offstage = offstage;
        }

        public bool offstage {
            get { return this._offstage; }

            set {
                if (value == this._offstage) {
                    return;
                }

                this._offstage = value;
                this.markNeedsLayoutForSizedByParentChange();
            }
        }

        bool _offstage;

        protected override float computeMinIntrinsicWidth(float height) {
            if (this.offstage) {
                return 0.0f;
            }

            return base.computeMinIntrinsicWidth(height);
        }


        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.offstage) {
                return 0.0f;
            }

            return base.computeMaxIntrinsicWidth(height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.offstage) {
                return 0.0f;
            }

            return base.computeMinIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.offstage) {
                return 0.0f;
            }

            return base.computeMaxIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (this.offstage) {
                return null;
            }

            return base.computeDistanceToActualBaseline(baseline);
        }

        protected override bool sizedByParent {
            get { return this.offstage; }
        }

        protected override void performResize() {
            D.assert(this.offstage);
            this.size = this.constraints.smallest;
        }

        protected override void performLayout() {
            if (this.offstage) {
                this.child?.layout(this.constraints);
            }
            else {
                base.performLayout();
            }
        }

        public override bool hitTest(HitTestResult result, Offset position) {
            return !this.offstage && base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.offstage) {
                return;
            }

            base.paint(context, offset);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", this.offstage));
        }


        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (this.child == null) {
                return new List<DiagnosticsNode>();
            }

            return new List<DiagnosticsNode> {
                this.child.toDiagnosticsNode(
                    name: "child",
                    style: this.offstage ? DiagnosticsTreeStyle.offstage : DiagnosticsTreeStyle.sparse
                ),
            };
        }
    }

    public class RenderAbsorbPointer : RenderProxyBox {
        public RenderAbsorbPointer(
            RenderBox child = null,
            bool absorbing = true
        ) : base(child) {
            this._absorbing = absorbing;
        }


        public bool absorbing {
            get { return this._absorbing; }
            set { this._absorbing = value; }
        }

        bool _absorbing;

        public override bool hitTest(HitTestResult result, Offset position) {
            return this.absorbing
                ? this.size.contains(position)
                : base.hitTest(result, position: position);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("absorbing", this.absorbing));
        }
    }

    public class RenderMetaData : RenderProxyBoxWithHitTestBehavior {
        public RenderMetaData(
            object metaData,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(behavior, child) {
            this.metaData = metaData;
        }

        public object metaData;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("metaData", this.metaData));
        }
    }

    public class RenderLeaderLayer : RenderProxyBox {
        public RenderLeaderLayer(
            LayerLink link = null,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(link != null);
            this.link = link;
        }

        public LayerLink link {
            get { return this._link; }
            set {
                D.assert(value != null);
                if (this._link == value) {
                    return;
                }

                this._link = value;
                this.markNeedsPaint();
            }
        }

        LayerLink _link;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            context.pushLayer(new LeaderLayer(link: this.link, offset: offset), base.paint, Offset.zero);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", this.link));
        }
    }


    class RenderFollowerLayer : RenderProxyBox {
        public RenderFollowerLayer(LayerLink link,
            bool showWhenUnlinked = true,
            Offset offset = null,
            RenderBox child = null
        ) : base(child) {
            this.link = link;
            this.showWhenUnlinked = showWhenUnlinked;
            this.offset = offset;
        }

        public LayerLink link {
            get { return this._link; }
            set {
                D.assert(value != null);
                if (this._link == value) {
                    return;
                }

                this._link = value;
                this.markNeedsPaint();
            }
        }

        LayerLink _link;

        public bool showWhenUnlinked {
            get { return this._showWhenUnlinked; }
            set {
                if (this._showWhenUnlinked == value) {
                    return;
                }

                this._showWhenUnlinked = value;
                this.markNeedsPaint();
            }
        }

        bool _showWhenUnlinked;

        public Offset offset {
            get { return this._offset; }
            set {
                D.assert(value != null);
                if (this._offset == value) {
                    return;
                }

                this._offset = value;
                this.markNeedsPaint();
            }
        }

        Offset _offset;

        public override void detach() {
            this._layer = null;
            base.detach();
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        new FollowerLayer _layer;

        Matrix3 getCurrentTransform() {
            return this._layer?.getLastTransform() ?? Matrix3.I();
        }

        public override bool hitTest(HitTestResult result, Offset position) {
            return this.hitTestChildren(result, position: position);
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            Matrix3 inverse = Matrix3.I();
            if (!this.getCurrentTransform().invert(inverse)) {
                return false;
            }

            position = inverse.mapPoint(position);
            return base.hitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            this._layer = new FollowerLayer(
                link: this.link,
                showWhenUnlinked: this.showWhenUnlinked,
                linkedOffset: this.offset,
                unlinkedOffset: offset
            );
            context.pushLayer(this._layer,
                base.paint,
                Offset.zero,
                childPaintBounds: Rect.fromLTRB(
                    float.NegativeInfinity,
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    float.PositiveInfinity
                )
            );
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            transform.preConcat(this.getCurrentTransform());
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", this.link));
            properties.add(new DiagnosticsProperty<bool>("showWhenUnlinked", this.showWhenUnlinked));
            properties.add(new DiagnosticsProperty<Offset>("offset", this.offset));
            properties.add(new TransformProperty("current transform matrix", this.getCurrentTransform()));
        }
    }

    public class RenderAnnotatedRegion<T> : RenderProxyBox
        where T : class {
        public RenderAnnotatedRegion(
            T value = null,
            bool? sized = null,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(value != null);
            D.assert(sized != null);
            this._value = value;
            this._sized = sized.Value;
        }

        public T value {
            get { return this._value; }
            set {
                if (this._value == value) {
                    return;
                }

                this._value = value;
                this.markNeedsPaint();
            }
        }

        T _value;

        public bool sized {
            get { return this._sized; }
            set {
                if (this._sized == value) {
                    return;
                }

                this._sized = value;
                this.markNeedsPaint();
            }
        }

        bool _sized;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            AnnotatedRegionLayer<T> layer =
                new AnnotatedRegionLayer<T>(
                    value: this.value,
                    size: this.sized ? this.size : null,
                    offset: this.sized ? offset : null);

            context.pushLayer(layer, base.paint, offset);
        }
    }
}