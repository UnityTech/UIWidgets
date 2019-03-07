using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class ScrollbarUtils {
        public static readonly TimeSpan _kScrollbarFadeDuration = TimeSpan.FromMilliseconds(300);

        public static readonly TimeSpan _kScrollbarTimeToFade = TimeSpan.FromMilliseconds(600);

        public const float _kScrollbarThickness = 6.0f;
    }


    public class Scrollbar : StatefulWidget {
        public Scrollbar(
            Key key = null,
            Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _ScrollbarState();
        }
    }

    public class _ScrollbarState : TickerProviderStateMixin<Scrollbar> {
        public ScrollbarPainter _materialPainter;
        public TextDirection _textDirection;
        public Color _themeColor;

        public AnimationController _fadeoutAnimationController;
        public Animation<float> _FadeoutOpacityAnimation;
        public Timer _fadeoutTimer;

        public override void initState() {
            base.initState();
            this._fadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: ScrollbarUtils._kScrollbarFadeDuration
            );
            this._FadeoutOpacityAnimation = new CurvedAnimation(
                parent: this._fadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            ThemeData theme = Theme.of(this.context);

            this._themeColor = theme.highlightColor.withOpacity(1.0f);
            this._textDirection = Directionality.of(this.context);
            this._materialPainter = this._BuildMaterialScrollbarPainter();
        }

        public ScrollbarPainter _BuildMaterialScrollbarPainter() {
            return new ScrollbarPainter(
                color: this._themeColor,
                textDirection: this._textDirection,
                thickness: ScrollbarUtils._kScrollbarThickness,
                fadeoutOpacityAnimation: this._FadeoutOpacityAnimation
            );
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (notification is ScrollUpdateNotification || notification is OverscrollNotification) {
                if (this._fadeoutAnimationController.status != AnimationStatus.forward) {
                    this._fadeoutAnimationController.forward();
                }

                this._materialPainter.update(notification.metrics, notification.metrics.axisDirection);
                this._fadeoutTimer?.cancel();

                this._fadeoutTimer = Window.instance.run(ScrollbarUtils._kScrollbarTimeToFade, () => {
                    this._fadeoutAnimationController.reverse();
                    this._fadeoutTimer = null;
                });
            }

            return false;
        }

        public override void dispose() {
            this._fadeoutAnimationController.dispose();
            this._fadeoutTimer?.cancel();
            this._materialPainter?.dispose();

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: this._materialPainter,
                        child: new RepaintBoundary(
                            child: this.widget.child
                        )
                    )
                )
            );
        }
    }
}