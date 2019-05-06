using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public class BoxConstraintsTween : Tween<BoxConstraints> {
        public BoxConstraintsTween(
            BoxConstraints begin = null,
            BoxConstraints end = null
        ) : base(begin: begin, end: end) {
        }

        public override BoxConstraints lerp(float t) {
            return BoxConstraints.lerp(this.begin, this.end, t);
        }
    }


    public class DecorationTween : Tween<Decoration> {
        public DecorationTween(
            Decoration begin = null,
            Decoration end = null) : base(begin: begin, end: end) {
        }

        public override Decoration lerp(float t) {
            return Decoration.lerp(this.begin, this.end, t);
        }
    }


    public class EdgeInsetsTween : Tween<EdgeInsets> {
        public EdgeInsetsTween(
            EdgeInsets begin = null,
            EdgeInsets end = null) : base(begin: begin, end: end) {
        }

        public override EdgeInsets lerp(float t) {
            return EdgeInsets.lerp(this.begin, this.end, t);
        }
    }


    public class BorderRadiusTween : Tween<BorderRadius> {
        public BorderRadiusTween(
            BorderRadius begin = null,
            BorderRadius end = null) : base(begin: begin, end: end) {
        }

        public override BorderRadius lerp(float t) {
            return BorderRadius.lerp(this.begin, this.end, t);
        }
    }


    public class BorderTween : Tween<Border> {
        public BorderTween(
            Border begin = null,
            Border end = null) : base(begin: begin, end: end) {
        }

        public override Border lerp(float t) {
            return Border.lerp(this.begin, this.end, t);
        }
    }


    public class Matrix3Tween : Tween<Matrix3> {
        public Matrix3Tween(
            Matrix3 begin = null,
            Matrix3 end = null) : base(begin: begin, end: end) {
        }

        //todo:xingwei.zhu implement full matrix3 lerp
        public override Matrix3 lerp(float t) {
            D.assert(this.begin != null);
            D.assert(this.end != null);

            return t < 0.5 ? this.begin : this.end;
        }
    }

    public class TextStyleTween : Tween<TextStyle> {
        public TextStyleTween(
            TextStyle begin = null,
            TextStyle end = null) : base(begin: begin, end: end) {
        }

        public override TextStyle lerp(float t) {
            return TextStyle.lerp(this.begin, this.end, t);
        }
    }

    public abstract class ImplicitlyAnimatedWidget : StatefulWidget {
        public ImplicitlyAnimatedWidget(
            Key key = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key) {
            D.assert(duration != null);
            this.curve = curve ?? Curves.linear;
            this.duration = duration ?? TimeSpan.Zero;
        }

        public readonly Curve curve;

        public readonly TimeSpan duration;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("duration", (int) this.duration.TotalMilliseconds, unit: "ms"));
        }
    }


    public delegate Tween<T> TweenConstructor<T>(T targetValue);

    public interface TweenVisitor {
        Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor) where T2 : ImplicitlyAnimatedWidget;
    }

    public class TweenVisitorUpdateTween : TweenVisitor {
        public Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor)
            where T2 : ImplicitlyAnimatedWidget {
            state._updateTween(tween, targetValue);
            return tween;
        }
    }

    public class TweenVisitorCheckStartAnimation : TweenVisitor {
        public bool shouldStartAnimation;

        public TweenVisitorCheckStartAnimation() {
            this.shouldStartAnimation = false;
        }

        public Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor)
            where T2 : ImplicitlyAnimatedWidget {
            if (targetValue != null) {
                tween = tween ?? constructor(targetValue);
                if (state._shouldAnimateTween(tween, targetValue)) {
                    this.shouldStartAnimation = true;
                }
            }
            else {
                tween = null;
            }

            return tween;
        }
    }


    public abstract class ImplicitlyAnimatedWidgetState<T> : SingleTickerProviderStateMixin<T>
        where T : ImplicitlyAnimatedWidget {
        protected AnimationController controller {
            get { return this._controller; }
        }

        AnimationController _controller;

        public Animation<float> animation {
            get { return this._animation; }
        }

        Animation<float> _animation;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: this.widget.duration,
                debugLabel: "{" + this.widget.toStringShort() + "}",
                vsync: this
            );
            this._updateCurve();
            this._constructTweens();
            this.didUpdateTweens();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            if (this.widget.curve != ((ImplicitlyAnimatedWidget) oldWidget).curve) {
                this._updateCurve();
            }

            this._controller.duration = this.widget.duration;
            if (this._constructTweens()) {
                var visitor = new TweenVisitorUpdateTween();
                this.forEachTween(visitor);
                this._controller.setValue(0.0f);
                this._controller.forward();
                this.didUpdateTweens();
            }
        }

        void _updateCurve() {
            if (this.widget.curve != null) {
                this._animation = new CurvedAnimation(parent: this._controller, curve: this.widget.curve);
            }
            else {
                this._animation = this._controller;
            }
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public bool _shouldAnimateTween<T2>(Tween<T2> tween, T2 targetValue) {
            return !targetValue.Equals(tween.end == null ? tween.begin : tween.end);
        }

        public void _updateTween<T2>(Tween<T2> tween, T2 targetValue) {
            if (tween == null) {
                return;
            }

            tween.begin = tween.evaluate(this._animation);
            tween.end = targetValue;
        }

        bool _constructTweens() {
            var visitor = new TweenVisitorCheckStartAnimation();
            this.forEachTween(visitor);
            return visitor.shouldStartAnimation;
        }

        protected abstract void forEachTween(TweenVisitor visitor);

        protected virtual void didUpdateTweens() {
        }
    }


    public abstract class AnimatedWidgetBaseState<T> : ImplicitlyAnimatedWidgetState<T>
        where T : ImplicitlyAnimatedWidget {
        public override void initState() {
            base.initState();
            this.controller.addListener(this._handleAnimationChanged);
        }

        void _handleAnimationChanged() {
            this.setState(() => { });
        }
    }


    public class AnimatedContainer : ImplicitlyAnimatedWidget {
        public AnimatedContainer(
            Key key = null,
            Alignment alignment = null,
            EdgeInsets padding = null,
            Color color = null,
            Decoration decoration = null,
            Decoration foregroundDecoration = null,
            float? width = null,
            float? height = null,
            BoxConstraints constraints = null,
            EdgeInsets margin = null,
            Matrix3 transform = null,
            Widget child = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(duration != null);
            D.assert(margin == null || margin.isNonNegative);
            D.assert(padding == null || padding.isNonNegative);
            D.assert(decoration == null || decoration.debugAssertIsValid());
            D.assert(constraints == null || constraints.debugAssertIsValid());
            D.assert(color == null || decoration == null,
                () => "Cannot provide both a color and a decoration\n" +
                "The color argument is just a shorthand for \"decoration: new BoxDecoration(backgroundColor: color)\".");
            this.alignment = alignment;
            this.padding = padding;
            this.foregroundDecoration = foregroundDecoration;
            this.margin = margin;
            this.transform = transform;
            this.child = child;
            this.decoration = decoration ?? (color != null ? new BoxDecoration(color: color) : null);
            this.constraints =
                (width != null || height != null)
                    ? constraints?.tighten(width: width, height: height)
                      ?? BoxConstraints.tightFor(width: width, height: height)
                    : constraints;
        }

        public readonly Widget child;

        public readonly Alignment alignment;

        public readonly EdgeInsets padding;

        public readonly Decoration decoration;

        public readonly Decoration foregroundDecoration;

        public readonly BoxConstraints constraints;

        public readonly EdgeInsets margin;

        public readonly Matrix3 transform;


        public override State createState() {
            return new _AnimatedContainerState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment, showName: false,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<Decoration>("bg", this.decoration, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<Decoration>("fg", this.foregroundDecoration, defaultValue: null));
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints", this.constraints,
                defaultValue: null,
                showName: false));
            properties.add(new DiagnosticsProperty<EdgeInsets>("margin", this.margin, defaultValue: null));
            properties.add(ObjectFlagProperty<Matrix3>.has("transform", this.transform));
        }
    }

    class _AnimatedContainerState : AnimatedWidgetBaseState<AnimatedContainer> {
        AlignmentTween _alignment;
        EdgeInsetsTween _padding;
        DecorationTween _decoration;
        DecorationTween _foregroundDecoration;
        BoxConstraintsTween _constraints;
        EdgeInsetsTween _margin;
        Matrix3Tween _transform;


        protected override void forEachTween(TweenVisitor visitor) {
            this._alignment = (AlignmentTween) visitor.visit(this, this._alignment, this.widget.alignment,
                (Alignment value) => new AlignmentTween(begin: value));
            this._padding = (EdgeInsetsTween) visitor.visit(this, this._padding, this.widget.padding,
                (EdgeInsets value) => new EdgeInsetsTween(begin: value));
            this._decoration = (DecorationTween) visitor.visit(this, this._decoration, this.widget.decoration,
                (Decoration value) => new DecorationTween(begin: value));
            this._foregroundDecoration = (DecorationTween) visitor.visit(this, this._foregroundDecoration,
                this.widget.foregroundDecoration, (Decoration value) => new DecorationTween(begin: value));
            this._constraints = (BoxConstraintsTween) visitor.visit(this, this._constraints, this.widget.constraints,
                (BoxConstraints value) => new BoxConstraintsTween(begin: value));
            this._margin = (EdgeInsetsTween) visitor.visit(this, this._margin, this.widget.margin,
                (EdgeInsets value) => new EdgeInsetsTween(begin: value));
            this._transform = (Matrix3Tween) visitor.visit(this, this._transform, this.widget.transform,
                (Matrix3 value) => new Matrix3Tween(begin: value));
        }


        public override Widget build(BuildContext context) {
            return new Container(
                child: this.widget.child,
                alignment: this._alignment?.evaluate(this.animation),
                padding: this._padding?.evaluate(this.animation),
                decoration: this._decoration?.evaluate(this.animation),
                forgroundDecoration: this._foregroundDecoration?.evaluate(this.animation),
                constraints: this._constraints?.evaluate(this.animation),
                margin: this._margin?.evaluate(this.animation),
                transfrom: this._transform?.evaluate(this.animation)
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<AlignmentTween>("alignment", this._alignment, showName: false,
                defaultValue: null));
            description.add(new DiagnosticsProperty<EdgeInsetsTween>("padding", this._padding, defaultValue: null));
            description.add(new DiagnosticsProperty<DecorationTween>("bg", this._decoration, defaultValue: null));
            description.add(
                new DiagnosticsProperty<DecorationTween>("fg", this._foregroundDecoration, defaultValue: null));
            description.add(new DiagnosticsProperty<BoxConstraintsTween>("constraints", this._constraints,
                showName: false, defaultValue: null));
            description.add(new DiagnosticsProperty<EdgeInsetsTween>("margin", this._margin, defaultValue: null));
            description.add(ObjectFlagProperty<Matrix3Tween>.has("transform", this._transform));
        }
    }

    public class AnimatedPadding : ImplicitlyAnimatedWidget {
        public AnimatedPadding(
            Key key = null,
            EdgeInsets padding = null,
            Widget child = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve, duration: duration) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);
            this.padding = padding;
            this.child = child;
        }

        public readonly EdgeInsets padding;

        public readonly Widget child;

        public override State createState() {
            return new _AnimatedPaddingState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
        }
    }

    class _AnimatedPaddingState : AnimatedWidgetBaseState<AnimatedPadding> {
        EdgeInsetsTween _padding;

        protected override void forEachTween(TweenVisitor visitor) {
            this._padding = (EdgeInsetsTween) visitor.visit(this, this._padding, this.widget.padding,
                (EdgeInsets value) => new EdgeInsetsTween(begin: value));
        }

        public override Widget build(BuildContext context) {
            return new Padding(
                padding: this._padding.evaluate(this.animation),
                child: this.widget.child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<EdgeInsetsTween>("padding", this._padding,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public class AnimatedOpacity : ImplicitlyAnimatedWidget {
        public AnimatedOpacity(
            Key key = null,
            Widget child = null,
            float? opacity = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) :
            base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(opacity != null && opacity >= 0.0 && opacity <= 1.0);
            this.child = child;
            this.opacity = opacity ?? 1.0f;
        }

        public readonly Widget child;

        public readonly float opacity;

        public override State createState() {
            return new _AnimatedOpacityState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("opacity", this.opacity));
        }
    }

    class _AnimatedOpacityState : ImplicitlyAnimatedWidgetState<AnimatedOpacity> {
        NullableFloatTween _opacity;
        Animation<float> _opacityAnimation;

        protected override void forEachTween(TweenVisitor visitor) {
            this._opacity = (NullableFloatTween) visitor.visit(this, this._opacity, this.widget.opacity,
                (float? value) => new NullableFloatTween(begin: value));
        }

        protected override void didUpdateTweens() {
            float? endValue = this._opacity.end ?? this._opacity.begin ?? null;
            D.assert(endValue != null);
            this._opacityAnimation = this.animation.drive(new FloatTween(begin: this._opacity.begin.Value, end: endValue.Value));
        }

        public override Widget build(BuildContext context) {
            return new FadeTransition(
                opacity: this._opacityAnimation,
                child: this.widget.child
            );
        }
    }

    public class AnimatedDefaultTextStyle : ImplicitlyAnimatedWidget {
        public AnimatedDefaultTextStyle(
            Key key = null,
            Widget child = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool softWrap = true,
            TextOverflow? overflow = null,
            int? maxLines = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(duration != null);
            D.assert(style != null);
            D.assert(child != null);
            D.assert(maxLines == null || maxLines > 0);
            this.child = child;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow ?? TextOverflow.clip;
            this.maxLines = maxLines;
        }

        public readonly Widget child;

        public readonly TextStyle style;

        public readonly bool softWrap;

        public readonly TextAlign? textAlign;

        public readonly TextOverflow overflow;

        public readonly int? maxLines;

        public override State createState() {
            return new _AnimatedDefaultTextStyleState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            this.style?.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", this.textAlign ?? TextAlign.center,
                defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: this.softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", this.overflow, defaultValue: null));
            properties.add(new IntProperty("maxLines", this.maxLines, defaultValue: null));
        }
    }


    class _AnimatedDefaultTextStyleState : AnimatedWidgetBaseState<AnimatedDefaultTextStyle> {
        TextStyleTween _style;

        protected override void forEachTween(TweenVisitor visitor) {
            this._style = (TextStyleTween) visitor.visit(this, this._style, this.widget.style,
                (TextStyle value) => new TextStyleTween(begin: value));
        }

        public override Widget build(BuildContext context) {
            return new DefaultTextStyle(
                style: this._style.evaluate(this.animation),
                textAlign: this.widget.textAlign,
                softWrap: this.widget.softWrap,
                overflow: this.widget.overflow,
                maxLines: this.widget.maxLines,
                child: this.widget.child);
        }
    }


    public class AnimatedPhysicalModel : ImplicitlyAnimatedWidget {
        public AnimatedPhysicalModel(
            Key key = null,
            Widget child = null,
            BoxShape? shape = null,
            Clip clipBehavior = Clip.none,
            BorderRadius borderRadius = null,
            float? elevation = null,
            Color color = null,
            bool animateColor = true,
            Color shadowColor = null,
            bool animateShadowColor = true,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(child != null);
            D.assert(shape != null);
            D.assert(elevation != null && elevation >= 0.0f);
            D.assert(color != null);
            D.assert(shadowColor != null);
            D.assert(duration != null);
            this.child = child;
            this.shape = shape ?? BoxShape.circle;
            this.clipBehavior = clipBehavior;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.elevation = elevation ?? 0.0f;
            this.color = color;
            this.animateColor = animateColor;
            this.shadowColor = shadowColor;
            this.animateShadowColor = animateShadowColor;
        }

        public readonly Widget child;

        public readonly BoxShape shape;

        public readonly Clip clipBehavior;

        public readonly BorderRadius borderRadius;

        public readonly float elevation;

        public readonly Color color;

        public readonly bool animateColor;

        public readonly Color shadowColor;

        public readonly bool animateShadowColor;

        public override State createState() {
            return new _AnimatedPhysicalModelState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxShape>("shape", this.shape));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius));
            properties.add(new FloatProperty("elevation", this.elevation));
            properties.add(new DiagnosticsProperty<Color>("color", this.color));
            properties.add(new DiagnosticsProperty<bool>("animateColor", this.animateColor));
            properties.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
            properties.add(new DiagnosticsProperty<bool>("animateShadowColor", this.animateShadowColor));
        }
    }

    class _AnimatedPhysicalModelState : AnimatedWidgetBaseState<AnimatedPhysicalModel> {
        BorderRadiusTween _borderRadius;
        FloatTween _elevation;
        ColorTween _color;
        ColorTween _shadowColor;

        protected override void forEachTween(TweenVisitor visitor) {
            this._borderRadius = (BorderRadiusTween) visitor.visit(this, this._borderRadius, this.widget.borderRadius,
                (BorderRadius value) => new BorderRadiusTween(begin: value));
            this._elevation = (FloatTween) visitor.visit(this, this._elevation, this.widget.elevation,
                (float value) => new FloatTween(begin: value, end: value));
            this._color = (ColorTween) visitor.visit(this, this._color, this.widget.color,
                (Color value) => new ColorTween(begin: value));
            this._shadowColor = (ColorTween) visitor.visit(this, this._shadowColor, this.widget.shadowColor,
                (Color value) => new ColorTween(begin: value));
        }

        public override Widget build(BuildContext context) {
            return new PhysicalModel(
                child: this.widget.child,
                shape: this.widget.shape,
                clipBehavior: this.widget.clipBehavior,
                borderRadius: this._borderRadius.evaluate(this.animation),
                elevation: this._elevation.evaluate(this.animation),
                color: this.widget.animateColor ? this._color.evaluate(this.animation) : this.widget.color,
                shadowColor: this.widget.animateShadowColor
                    ? this._shadowColor.evaluate(this.animation)
                    : this.widget.shadowColor);
        }
    }
}