using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class TooltipUtils {
        public static readonly TimeSpan _kFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly TimeSpan _kShowDuration = new TimeSpan(0, 0, 0, 0, 1500);
    }


    public class Tooltip : StatefulWidget {
        public Tooltip(
            Key key = null,
            string message = null,
            float height = 32.0f,
            EdgeInsets padding = null,
            float verticalOffset = 24.0f,
            bool preferBelow = true,
            Widget child = null
        ) : base(key: key) {
            D.assert(message != null);
            this.message = message;
            this.height = height;
            this.padding = padding ?? EdgeInsets.symmetric(horizontal: 16.0f);
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
            this.child = child;
        }


        public readonly string message;

        public readonly float height;

        public readonly EdgeInsets padding;

        public readonly float verticalOffset;

        public readonly bool preferBelow;

        public readonly Widget child;

        public override State createState() {
            return new _TooltipState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("message", this.message, showName: false));
            properties.add(new FloatProperty("vertical offset", this.verticalOffset));
            properties.add(new FlagProperty("position", value: this.preferBelow, ifTrue: "below", ifFalse: "above",
                showName: true));
        }
    }


    public class _TooltipState : SingleTickerProviderStateMixin<Tooltip> {
        AnimationController _controller;
        OverlayEntry _entry;
        Timer _timer;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: TooltipUtils._kFadeDuration, vsync: this);
            this._controller.addStatusListener(this._handleStatusChanged);
        }

        void _handleStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.dismissed) {
                this._removeEntry();
            }
        }

        bool ensureTooltipVisible() {
            if (this._entry != null) {
                this._timer?.cancel();
                this._timer = null;
                this._controller.forward();
                return false;
            }

            RenderBox box = (RenderBox) this.context.findRenderObject();
            Offset target = box.localToGlobal(box.size.center(Offset.zero));

            Widget overlay = new _TooltipOverlay(
                message: this.widget.message,
                height: this.widget.height,
                padding: this.widget.padding,
                animation: new CurvedAnimation(
                    parent: this._controller,
                    curve: Curves.fastOutSlowIn),
                target: target,
                verticalOffset: this.widget.verticalOffset,
                preferBelow: this.widget.preferBelow
            );

            this._entry = new OverlayEntry(builder: (BuildContext context) => overlay);
            Overlay.of(this.context, debugRequiredFor: this.widget).insert(this._entry);
            GestureBinding.instance.pointerRouter.addGlobalRoute(this._handlePointerEvent);
            this._controller.forward();
            return true;
        }

        void _removeEntry() {
            D.assert(this._entry != null);
            this._timer?.cancel();
            this._timer = null;
            this._entry.remove();
            this._entry = null;
            GestureBinding.instance.pointerRouter.removeGlobalRoute(this._handlePointerEvent);
        }

        void _handlePointerEvent(PointerEvent pEvent) {
            D.assert(this._entry != null);
            if (pEvent is PointerUpEvent || pEvent is PointerCancelEvent) {
                this._timer = this._timer ?? Window.instance.run(TooltipUtils._kShowDuration,
                                  () => this._controller.reverse());
            }
            else if (pEvent is PointerDownEvent) {
                this._controller.reverse();
            }
        }

        public override void deactivate() {
            if (this._entry != null) {
                this._controller.reverse();
            }

            base.deactivate();
        }

        public override void dispose() {
            if (this._entry != null) {
                this._removeEntry();
            }

            this._controller.dispose();
            base.dispose();
        }

        void _handleLongPress() {
            bool tooltipCreated = this.ensureTooltipVisible();
            if (tooltipCreated) {
                Feedback.forLongPress(this.context);
            }
        }


        public override Widget build(BuildContext context) {
            D.assert(Overlay.of(context, debugRequiredFor: this.widget) != null);
            return new GestureDetector(
                behavior: HitTestBehavior.opaque,
                onLongPress: this._handleLongPress,
                child: this.widget.child
            );
        }
    }


    public class _TooltipPositionDelegate : SingleChildLayoutDelegate {
        public _TooltipPositionDelegate(
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null) {
            D.assert(target != null);
            D.assert(verticalOffset != null);
            D.assert(preferBelow != null);
            this.target = target;
            this.verticalOffset = verticalOffset ?? 0.0f;
            this.preferBelow = preferBelow ?? true;
        }

        public readonly Offset target;

        public readonly float verticalOffset;

        public readonly bool preferBelow;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return Geometry.positionDependentBox(
                size: size,
                childSize: childSize,
                target: this.target,
                verticalOffset: this.verticalOffset,
                preferBelow: this.preferBelow);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            _TooltipPositionDelegate _oldDelegate = (_TooltipPositionDelegate) oldDelegate;
            return this.target != _oldDelegate.target ||
                   this.verticalOffset != _oldDelegate.verticalOffset ||
                   this.preferBelow != _oldDelegate.preferBelow;
        }
    }


    class _TooltipOverlay : StatelessWidget {
        public _TooltipOverlay(
            Key key = null,
            string message = null,
            float? height = null,
            EdgeInsets padding = null,
            Animation<float> animation = null,
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null
        ) : base(key: key) {
            this.message = message;
            this.height = height;
            this.padding = padding;
            this.animation = animation;
            this.target = target;
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
        }

        public readonly string message;

        public readonly float? height;

        public readonly EdgeInsets padding;

        public readonly Animation<float> animation;

        public readonly Offset target;

        public readonly float? verticalOffset;

        public readonly bool? preferBelow;


        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ThemeData darkTheme = new ThemeData(
                brightness: Brightness.dark,
                textTheme: theme.brightness == Brightness.dark ? theme.textTheme : theme.primaryTextTheme
            );
            return Positioned.fill(
                child: new IgnorePointer(
                    child: new CustomSingleChildLayout(
                        layoutDelegate: new _TooltipPositionDelegate(
                            target: this.target,
                            verticalOffset: this.verticalOffset,
                            preferBelow: this.preferBelow),
                        child: new FadeTransition(
                            opacity: this.animation,
                            child: new Opacity(
                                opacity: 0.9f,
                                child: new ConstrainedBox(
                                    constraints: new BoxConstraints(minHeight: this.height ?? 0.0f),
                                    child: new Container(
                                        decoration: new BoxDecoration(
                                            color: darkTheme.backgroundColor,
                                            borderRadius: BorderRadius.circular(2.0f)),
                                        padding: this.padding,
                                        child: new Center(
                                            widthFactor: 1.0f,
                                            heightFactor: 1.0f,
                                            child: new Text(this.message, style: darkTheme.textTheme.body1)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}