using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public enum CrossFadeState {
        showFirst,
        showSecond
    }

    public delegate Widget AnimatedCrossFadeBuilder(Widget topChild, Key topChildKey, Widget bottomChild,
        Key bottomChildKey);


    public class AnimatedCrossFade : StatefulWidget {
        public AnimatedCrossFade(
            Key key = null,
            Widget firstChild = null,
            Widget secondChild = null,
            Curve firstCurve = null,
            Curve secondCurve = null,
            Curve sizeCurve = null,
            Alignment alignment = null,
            CrossFadeState? crossFadeState = null,
            TimeSpan? duration = null,
            AnimatedCrossFadeBuilder layoutBuilder = null
        ) : base(key: key) {
            D.assert(firstChild != null);
            D.assert(secondChild != null);
            D.assert(crossFadeState != null);
            D.assert(duration != null);
            this.firstChild = firstChild;
            this.secondChild = secondChild;
            this.firstCurve = firstCurve ?? Curves.linear;
            this.secondCurve = secondCurve ?? Curves.linear;
            this.sizeCurve = sizeCurve ?? Curves.linear;
            this.alignment = alignment ?? Alignment.topCenter;
            this.crossFadeState = crossFadeState ?? CrossFadeState.showFirst;
            this.duration = duration ?? TimeSpan.Zero;
            this.layoutBuilder = layoutBuilder ?? defaultLayoutBuilder;
        }

        public readonly Widget firstChild;

        public readonly Widget secondChild;

        public readonly CrossFadeState crossFadeState;

        public readonly TimeSpan duration;

        public readonly Curve firstCurve;

        public readonly Curve secondCurve;

        public readonly Curve sizeCurve;

        public readonly Alignment alignment;

        public readonly AnimatedCrossFadeBuilder layoutBuilder;

        static Widget defaultLayoutBuilder(Widget topChild, Key topChildKey, Widget bottomChild, Key bottomChildKey) {
            return new Stack(
                overflow: Overflow.visible,
                children: new List<Widget> {
                    new Positioned(
                        key: bottomChildKey,
                        left: 0.0f,
                        top: 0.0f,
                        right: 0.0f,
                        child: bottomChild),
                    new Positioned(
                        key: topChildKey,
                        child: topChild)
                }
            );
        }

        public override State createState() {
            return new _AnimatedCrossFadeState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<CrossFadeState>("crossFadeState", this.crossFadeState));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment,
                defaultValue: Alignment.topCenter));
        }
    }


    public class _AnimatedCrossFadeState : TickerProviderStateMixin<AnimatedCrossFade> {
        AnimationController _controller;
        Animation<float> _firstAnimation;
        Animation<float> _secondAnimation;


        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: this.widget.duration, vsync: this);
            if (this.widget.crossFadeState == CrossFadeState.showSecond) {
                this._controller.setValue(1.0f);
            }

            this._firstAnimation = this._initAnimation(this.widget.firstCurve, true);
            this._secondAnimation = this._initAnimation(this.widget.secondCurve, false);
            this._controller.addStatusListener((AnimationStatus status) => { this.setState(() => { }); });
        }

        Animation<float> _initAnimation(Curve curve, bool inverted) {
            Animation<float> result = this._controller.drive(new CurveTween(curve: curve));
            if (inverted) {
                result = result.drive(new FloatTween(begin: 1.0f, end: 0.0f));
            }

            return result;
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            AnimatedCrossFade _oldWidget = (AnimatedCrossFade) oldWidget;
            if (this.widget.duration != _oldWidget.duration) {
                this._controller.duration = this.widget.duration;
            }

            if (this.widget.firstCurve != _oldWidget.firstCurve) {
                this._firstAnimation = this._initAnimation(this.widget.firstCurve, true);
            }

            if (this.widget.secondCurve != _oldWidget.secondCurve) {
                this._secondAnimation = this._initAnimation(this.widget.secondCurve, false);
            }

            if (this.widget.crossFadeState != _oldWidget.crossFadeState) {
                switch (this.widget.crossFadeState) {
                    case CrossFadeState.showFirst:
                        this._controller.reverse();
                        break;
                    case CrossFadeState.showSecond:
                        this._controller.forward();
                        break;
                }
            }
        }

        bool _isTransitioning {
            get {
                return this._controller.status == AnimationStatus.forward ||
                       this._controller.status == AnimationStatus.reverse;
            }
        }

        public override Widget build(BuildContext context) {
            Key kFirstChildKey = new ValueKey<CrossFadeState>(CrossFadeState.showFirst);
            Key kSecondChildKey = new ValueKey<CrossFadeState>(CrossFadeState.showSecond);
            bool transitioningForwards = this._controller.status == AnimationStatus.completed ||
                                         this._controller.status == AnimationStatus.forward;

            Key topKey;
            Widget topChild;
            Animation<float> topAnimation;
            Key bottomKey;
            Widget bottomChild;
            Animation<float> bottomAnimation;
            if (transitioningForwards) {
                topKey = kSecondChildKey;
                topChild = this.widget.secondChild;
                topAnimation = this._secondAnimation;
                bottomKey = kFirstChildKey;
                bottomChild = this.widget.firstChild;
                bottomAnimation = this._firstAnimation;
            }
            else {
                topKey = kFirstChildKey;
                topChild = this.widget.firstChild;
                topAnimation = this._firstAnimation;
                bottomKey = kSecondChildKey;
                bottomChild = this.widget.secondChild;
                bottomAnimation = this._secondAnimation;
            }

            bottomChild = new TickerMode(
                key: bottomKey,
                enabled: this._isTransitioning,
                child: new FadeTransition(
                    opacity: bottomAnimation,
                    child: bottomChild
                )
            );

            topChild = new TickerMode(
                key: topKey,
                enabled: true,
                child: new FadeTransition(
                    opacity: topAnimation,
                    child: topChild
                )
            );

            return new ClipRect(
                child: new AnimatedSize(
                    alignment: this.widget.alignment,
                    duration: this.widget.duration,
                    curve: this.widget.sizeCurve,
                    vsync: this,
                    child: this.widget.layoutBuilder(topChild, topKey, bottomChild, bottomKey)
                )
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new EnumProperty<CrossFadeState>("crossFadeState", this.widget.crossFadeState));
            description.add(
                new DiagnosticsProperty<AnimationController>("controller", this._controller, showName: false));
            description.add(new DiagnosticsProperty<Alignment>("alignment", this.widget.alignment,
                defaultValue: Alignment.topCenter));
        }
    }
}