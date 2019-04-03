using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    static class ToggleableUtils {
        public static readonly TimeSpan _kToggleDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static readonly Animatable<float> _kRadialReactionRadiusTween =
            new FloatTween(begin: 0.0f, end: Constants.kRadialReactionRadius);
    }

    public abstract class RenderToggleable : RenderConstrainedBox {
        protected RenderToggleable(
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color inactiveColor = null,
            ValueChanged<bool?> onChanged = null,
            BoxConstraints additionalConstraints = null,
            TickerProvider vsync = null
        ) : base(additionalConstraints: additionalConstraints) {
            D.assert(tristate || value != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(vsync != null);
            this._value = value;
            this._tristate = tristate;
            this._activeColor = activeColor;
            this._inactiveColor = inactiveColor;
            this._onChanged = onChanged;
            this._vsync = vsync;

            this._tap = new TapGestureRecognizer {
                onTapDown = this._handleTapDown,
                onTap = this._handleTap,
                onTapUp = this._handleTapUp,
                onTapCancel = this._handleTapCancel
            };

            this._positionController = new AnimationController(
                duration: ToggleableUtils._kToggleDuration,
                value: value == false ? 0.0f : 1.0f,
                vsync: vsync);

            this._position = new CurvedAnimation(
                parent: this._positionController,
                curve: Curves.linear);
            this._position.addListener(this.markNeedsPaint);
            this._position.addStatusListener(this._handlePositionStateChanged);

            this._reactionController = new AnimationController(
                duration: Constants.kRadialReactionDuration,
                vsync: vsync);

            this._reaction = new CurvedAnimation(
                parent: this._reactionController,
                curve: Curves.fastOutSlowIn);
            this._reaction.addListener(this.markNeedsPaint);
        }

        protected AnimationController positionController {
            get { return this._positionController; }
        }

        AnimationController _positionController;

        public CurvedAnimation position {
            get { return this._position; }
        }

        CurvedAnimation _position;

        protected AnimationController reactionController {
            get { return this._reactionController; }
        }

        AnimationController _reactionController;

        Animation<float> _reaction;

        public TickerProvider vsync {
            get { return this._vsync; }
            set {
                D.assert(value != null);
                if (value == this._vsync) {
                    return;
                }

                this._vsync = value;
                this.positionController.resync(this.vsync);
                this.reactionController.resync(this.vsync);
            }
        }

        TickerProvider _vsync;

        public virtual bool? value {
            get { return this._value; }
            set {
                D.assert(this.tristate || value != null);
                if (value == this._value) {
                    return;
                }

                this._value = value;
                this._position.curve = Curves.easeIn;
                this._position.reverseCurve = Curves.easeOut;
                if (this.tristate) {
                    switch (this._positionController.status) {
                        case AnimationStatus.forward:
                        case AnimationStatus.completed: {
                            this._positionController.reverse();
                            break;
                        }
                        default: {
                            this._positionController.forward();
                            break;
                        }
                    }
                }
                else {
                    if (value == true) {
                        this._positionController.forward();
                    }
                    else {
                        this._positionController.reverse();
                    }
                }
            }
        }

        bool? _value;

        public bool tristate {
            get { return this._tristate; }
            set {
                if (value == this._tristate) {
                    return;
                }

                this._tristate = value;
            }
        }

        bool _tristate;

        public Color activeColor {
            get { return this._activeColor; }
            set {
                D.assert(value != null);
                if (value == this._activeColor) {
                    return;
                }

                this._activeColor = value;
                this.markNeedsPaint();
            }
        }

        Color _activeColor;

        public Color inactiveColor {
            get { return this._inactiveColor; }
            set {
                D.assert(value != null);
                if (value == this._inactiveColor) {
                    return;
                }

                this._inactiveColor = value;
                this.markNeedsPaint();
            }
        }

        Color _inactiveColor;

        public ValueChanged<bool?> onChanged {
            get { return this._onChanged; }
            set {
                if (value == this._onChanged) {
                    return;
                }

                bool wasInteractive = this.isInteractive;
                this._onChanged = value;
                if (wasInteractive != this.isInteractive) {
                    this.markNeedsPaint();
                }
            }
        }

        ValueChanged<bool?> _onChanged;

        public bool isInteractive {
            get { return this.onChanged != null; }
        }

        TapGestureRecognizer _tap;
        Offset _downPosition;

        public override void attach(object owner) {
            base.attach(owner);
            if (this.value == false) {
                this._positionController.reverse();
            }
            else {
                this._positionController.forward();
            }

            if (this.isInteractive) {
                switch (this._reactionController.status) {
                    case AnimationStatus.forward: {
                        this._reactionController.forward();
                        break;
                    }
                    case AnimationStatus.reverse: {
                        this._reactionController.reverse();
                        break;
                    }
                    case AnimationStatus.dismissed:
                    case AnimationStatus.completed: {
                        break;
                    }
                }
            }
        }

        public override void detach() {
            this._positionController.stop();
            this._reactionController.stop();
            base.detach();
        }

        void _handlePositionStateChanged(AnimationStatus status) {
            if (this.isInteractive && !this.tristate) {
                if (status == AnimationStatus.completed && this._value == false) {
                    this.onChanged(true);
                }
                else if (status == AnimationStatus.dismissed && this._value != false) {
                    this.onChanged(false);
                }
            }
        }

        void _handleTapDown(TapDownDetails details) {
            if (this.isInteractive) {
                this._downPosition = this.globalToLocal(details.globalPosition);
                this._reactionController.forward();
            }
        }

        void _handleTap() {
            if (!this.isInteractive) {
                return;
            }

            switch (this.value) {
                case false:
                    this.onChanged(true);
                    break;
                case true:
                    this.onChanged(this.tristate ? (bool?) null : false);
                    break;
                default:
                    this.onChanged(false);
                    break;
            }
        }

        void _handleTapUp(TapUpDetails details) {
            this._downPosition = null;
            if (this.isInteractive) {
                this._reactionController.reverse();
            }
        }

        void _handleTapCancel() {
            this._downPosition = null;
            if (this.isInteractive) {
                this._reactionController.reverse();
            }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent pEvent, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(pEvent, entry));
            if (pEvent is PointerDownEvent && this.isInteractive) {
                this._tap.addPointer((PointerDownEvent) pEvent);
            }
        }

        public void paintRadialReaction(Canvas canvas, Offset offset, Offset origin) {
            if (!this._reaction.isDismissed) {
                Paint reactionPaint = new Paint {color = this.activeColor.withAlpha(Constants.kRadialReactionAlpha)};
                Offset center = Offset.lerp(this._downPosition ?? origin, origin, this._reaction.value);
                float radius = ToggleableUtils._kRadialReactionRadiusTween.evaluate(this._reaction);
                canvas.drawCircle(center + offset, radius, reactionPaint);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: this.value, ifTrue: "checked", ifFalse: "unchecked",
                showName: true));
            properties.add(new FlagProperty("isInteractive", value: this.isInteractive, ifTrue: "enabled",
                ifFalse: "disabled", defaultValue: true));
        }
    }
}