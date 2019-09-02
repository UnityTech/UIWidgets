using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    class CupertinoScrollbarUtils {
        public static readonly Color _kScrollbarColor = new Color(0x99777777);
        public const float _kScrollbarThickness = 2.5f;
        public const float _kScrollbarMainAxisMargin = 4.0f;
        public const float _kScrollbarCrossAxisMargin = 2.5f;
        public const float _kScrollbarMinLength = 36.0f;
        public const float _kScrollbarMinOverscrollLength = 8.0f;
        public static readonly Radius _kScrollbarRadius = Radius.circular(1.25f);
        public static readonly TimeSpan _kScrollbarTimeToFade = new TimeSpan(0, 0, 0, 0, 50);
        public static readonly TimeSpan _kScrollbarFadeDuration = new TimeSpan(0, 0, 0, 0, 250);
    }

    public class CupertinoScrollbar : StatefulWidget {
        public CupertinoScrollbar(
            Widget child,
            Key key = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _CupertinoScrollbarState();
        }
    }

    class _CupertinoScrollbarState : TickerProviderStateMixin<CupertinoScrollbar> {
        ScrollbarPainter _painter;
        TextDirection _textDirection;
        AnimationController _fadeoutAnimationController;
        Animation<float> _fadeoutOpacityAnimation;
        Timer _fadeoutTimer;

        public override void initState() {
            base.initState();
            this._fadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: CupertinoScrollbarUtils._kScrollbarFadeDuration
            );
            this._fadeoutOpacityAnimation = new CurvedAnimation(
                parent: this._fadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
        }


        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._textDirection = Directionality.of(this.context);
            this._painter = this._buildCupertinoScrollbarPainter();
        }

        ScrollbarPainter _buildCupertinoScrollbarPainter() {
            return new ScrollbarPainter(
                color: CupertinoScrollbarUtils._kScrollbarColor,
                textDirection: this._textDirection,
                thickness: CupertinoScrollbarUtils._kScrollbarThickness,
                fadeoutOpacityAnimation: this._fadeoutOpacityAnimation,
                mainAxisMargin: CupertinoScrollbarUtils._kScrollbarMainAxisMargin,
                crossAxisMargin: CupertinoScrollbarUtils._kScrollbarCrossAxisMargin,
                radius: CupertinoScrollbarUtils._kScrollbarRadius,
                minLength: CupertinoScrollbarUtils._kScrollbarMinLength,
                minOverscrollLength: CupertinoScrollbarUtils._kScrollbarMinOverscrollLength
            );
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (notification is ScrollUpdateNotification ||
                notification is OverscrollNotification) {
                if (this._fadeoutAnimationController.status != AnimationStatus.forward) {
                    this._fadeoutAnimationController.forward();
                }

                this._fadeoutTimer?.cancel();
                this._painter.update(notification.metrics, notification.metrics.axisDirection);
            }
            else if (notification is ScrollEndNotification) {
                this._fadeoutTimer?.cancel();
                this._fadeoutTimer = Window.instance.run(CupertinoScrollbarUtils._kScrollbarTimeToFade, () => {
                    this._fadeoutAnimationController.reverse();
                    this._fadeoutTimer = null;
                });
            }

            return false;
        }


        public override void dispose() {
            this._fadeoutAnimationController.dispose();
            this._fadeoutTimer?.cancel();
            this._painter.dispose();
            base.dispose();
        }


        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: this._painter,
                        child: new RepaintBoundary(
                            child: this.widget.child
                        )
                    )
                )
            );
        }
    }
}