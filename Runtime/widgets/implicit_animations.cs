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

        public override BoxConstraints lerp(double t) => BoxConstraints.lerp(this.begin, this.end, t);
    }

    public class BorderRadiusTween : Tween<BorderRadius> {
        public BorderRadiusTween(
            BorderRadius begin = null,
            BorderRadius end = null) : base(begin: begin, end: end) {
        }

        public override BorderRadius lerp(double t) => BorderRadius.lerp(this.begin, this.end, t);
    }

    public class TextStyleTween : Tween<TextStyle> {
        public TextStyleTween(
            TextStyle begin = null,
            TextStyle end = null) : base(begin: begin, end: end) {
        }

        public override TextStyle lerp(double t) => TextStyle.lerp(this.begin, this.end, t);
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

    public interface ITweenVisitor {
        Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor) where T2 : ImplicitlyAnimatedWidget;
    }

    public class TweenVisitorUpdateTween : ITweenVisitor {
        public Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor)
            where T2 : ImplicitlyAnimatedWidget {
            state._updateTween(tween, targetValue);
            return tween;
        }
    }

    public class TweenVisitorCheckStartAnimation : ITweenVisitor {
        public bool shouldStartAnimation;

        public TweenVisitorCheckStartAnimation() {
            this.shouldStartAnimation = false;
        }

        public Tween<T> visit<T, T2>(ImplicitlyAnimatedWidgetState<T2> state, Tween<T> tween, T targetValue,
            TweenConstructor<T> constructor)
            where T2 : ImplicitlyAnimatedWidget {
            if (targetValue != null) {
                tween = tween ?? constructor(targetValue);
                if (state._shouldAnimateTween(tween, targetValue))
                    this.shouldStartAnimation = true;
            }
            else {
                tween = null;
            }

            return tween;
        }
    }


    public abstract class ImplicitlyAnimatedWidgetState<T> : SingleTickerProviderStateMixin<T>
        where T : ImplicitlyAnimatedWidget {
        protected AnimationController controller => this._controller;

        AnimationController _controller;

        public Animation<double> animation => this._animation;

        Animation<double> _animation;

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

            if (this.widget.curve != ((ImplicitlyAnimatedWidget) oldWidget).curve)
                this._updateCurve();

            this._controller.duration = this.widget.duration;
            if (this._constructTweens()) {
                var visitor = new TweenVisitorUpdateTween();
                this.forEachTween(visitor);
                this._controller.setValue(0.0);
                this._controller.forward();
                this.didUpdateTweens();
            }
        }

        void _updateCurve() {
            if (this.widget.curve != null)
                this._animation = new CurvedAnimation(parent: this._controller, curve: this.widget.curve);
            else
                this._animation = this._controller;
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public bool _shouldAnimateTween<T2>(Tween<T2> tween, T2 targetValue) {
            return !targetValue.Equals(tween.end == null ? tween.begin : tween.end);
        }

        public void _updateTween<T2>(Tween<T2> tween, T2 targetValue) {
            if (tween == null)
                return;

            tween.begin = tween.evaluate(this._animation);
            tween.end = targetValue;
        }

        bool _constructTweens() {
            var visitor = new TweenVisitorCheckStartAnimation();
            this.forEachTween(visitor);
            return visitor.shouldStartAnimation;
        }

        protected abstract void forEachTween(ITweenVisitor visitor);

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

        public override State createState() => new _AnimatedDefaultTextStyleState();

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


    public class _AnimatedDefaultTextStyleState : AnimatedWidgetBaseState<AnimatedDefaultTextStyle> {
        TextStyleTween _style;

        protected override void forEachTween(ITweenVisitor visitor) {
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
            double? elevation = null,
            Color color = null,
            bool animateColor = true,
            Color shadowColor = null,
            bool animateShadowColor = true,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(child != null);
            D.assert(shape != null);
            D.assert(elevation != null);
            D.assert(color != null);
            D.assert(shadowColor != null);
            D.assert(duration != null);
            this.child = child;
            this.shape = shape ?? BoxShape.circle;
            this.clipBehavior = clipBehavior;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.elevation = elevation ?? 0.0;
            this.color = color;
            this.animateColor = animateColor;
            this.shadowColor = shadowColor;
            this.animateShadowColor = animateShadowColor;
        }

        public readonly Widget child;

        public readonly BoxShape shape;

        public readonly Clip clipBehavior;

        public readonly BorderRadius borderRadius;

        public readonly double elevation;

        public readonly Color color;

        public readonly bool animateColor;

        public readonly Color shadowColor;

        public readonly bool animateShadowColor;

        public override State createState() => new _AnimatedPhysicalModelState();

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxShape>("shape", this.shape));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius));
            properties.add(new DoubleProperty("elevation", this.elevation));
            properties.add(new DiagnosticsProperty<Color>("color", this.color));
            properties.add(new DiagnosticsProperty<bool>("animateColor", this.animateColor));
            properties.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
            properties.add(new DiagnosticsProperty<bool>("animateShadowColor", this.animateShadowColor));
        }
    }

    public class _AnimatedPhysicalModelState : AnimatedWidgetBaseState<AnimatedPhysicalModel> {
        BorderRadiusTween _borderRadius;
        DoubleTween _elevation;
        ColorTween _color;
        ColorTween _shadowColor;

        protected override void forEachTween(ITweenVisitor visitor) {
            this._borderRadius = (BorderRadiusTween) visitor.visit(this, this._borderRadius, this.widget.borderRadius,
                (BorderRadius value) => new BorderRadiusTween(begin: value));
            this._elevation = (DoubleTween) visitor.visit(this, this._elevation, this.widget.elevation,
                (double value) => new DoubleTween(begin: value, end: value));
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