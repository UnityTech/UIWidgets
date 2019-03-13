using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class BottomSheetUtils {
        public static readonly TimeSpan _kBottomSheetDuration = new TimeSpan(0, 0, 0, 0, 200);
        public const float _kMinFlingVelocity = 700.0f;
        public const float _kCloseProgressThreshold = 0.5f;
    }


    public class BottomSheet : StatefulWidget {
        public BottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            float elevation = 0.0f,
            VoidCallback onClosing = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(onClosing != null);
            D.assert(builder != null);
            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.elevation = elevation;
            this.onClosing = onClosing;
            this.builder = builder;
        }

        public readonly AnimationController animationController;

        public readonly VoidCallback onClosing;

        public readonly WidgetBuilder builder;

        public readonly bool enableDrag;

        public readonly float elevation;

        public override State createState() {
            return new _BottomSheetState();
        }

        public static AnimationController createAnimationController(TickerProvider vsync) {
            return new AnimationController(
                duration: BottomSheetUtils._kBottomSheetDuration,
                debugLabel: "BottomSheet",
                vsync: vsync
            );
        }
    }


    class _BottomSheetState : State<BottomSheet> {
        readonly GlobalKey _childKey = GlobalKey.key(debugLabel: "BottomSheet child");

        float? _childHeight {
            get {
                RenderBox renderBox = (RenderBox) this._childKey.currentContext.findRenderObject();
                return renderBox.size.height;
            }
        }

        bool _dismissUnderway {
            get { return this.widget.animationController.status == AnimationStatus.reverse; }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (this._dismissUnderway) {
                return;
            }

            this.widget.animationController.setValue(
                this.widget.animationController.value -
                details.primaryDelta.Value / (this._childHeight ?? details.primaryDelta.Value));
        }

        void _handleDragEnd(DragEndDetails details) {
            if (this._dismissUnderway) {
                return;
            }

            if (details.velocity.pixelsPerSecond.dy > BottomSheetUtils._kMinFlingVelocity) {
                float flingVelocity = -details.velocity.pixelsPerSecond.dy / this._childHeight.Value;
                if (this.widget.animationController.value > 0.0f) {
                    this.widget.animationController.fling(velocity: flingVelocity);
                }

                if (flingVelocity < 0.0f) {
                    this.widget.onClosing();
                }
            }
            else if (this.widget.animationController.value < BottomSheetUtils._kCloseProgressThreshold) {
                if (this.widget.animationController.value > 0.0f) {
                    this.widget.animationController.fling(velocity: -1.0f);
                }

                this.widget.onClosing();
            }
            else {
                this.widget.animationController.forward();
            }
        }

        public override Widget build(BuildContext context) {
            Widget bottomSheet = new Material(
                key: this._childKey,
                elevation: this.widget.elevation,
                child: this.widget.builder(context)
            );

            return !this.widget.enableDrag
                ? bottomSheet
                : new GestureDetector(
                    onVerticalDragUpdate: this._handleDragUpdate,
                    onVerticalDragEnd: this._handleDragEnd,
                    child: bottomSheet
                );
        }
    }
}