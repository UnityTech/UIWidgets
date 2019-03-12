using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class SnackBarUtils {
        public const float _kSnackBarPadding = 24.0f;
        public const float _kSingleLineVerticalPadding = 14.0f;
        public static readonly Color _kSnackBackground = new Color(0xFF323232);

        public static readonly TimeSpan _kSnackBarTransitionDuration = new TimeSpan(0, 0, 0, 0, 250);
        public static readonly TimeSpan _kSnackBarDisplayDuration = new TimeSpan(0, 0, 0, 0, 4000);
        public static readonly Curve _snackBarHeightCurve = Curves.fastOutSlowIn;
        public static readonly Curve _snackBarFadeCurve = new Interval(0.72f, 1.0f, curve: Curves.fastOutSlowIn);
    }

    public enum SnackBarClosedReason {
        action,
        dismiss,
        swipe,
        hide,
        remove,
        timeout
    }

    public class SnackBarAction : StatefulWidget {
        public SnackBarAction(
            Key key = null,
            Color textColor = null,
            Color disabledTextColor = null,
            string label = null,
            VoidCallback onPressed = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(onPressed != null);
            this.textColor = textColor;
            this.disabledTextColor = disabledTextColor;
            this.label = label;
            this.onPressed = onPressed;
        }

        public readonly Color textColor;

        public readonly Color disabledTextColor;

        public readonly string label;

        public readonly VoidCallback onPressed;

        public override State createState() {
            return new _SnackBarActionState();
        }
    }


    class _SnackBarActionState : State<SnackBarAction> {
        bool _haveTriggeredAction = false;

        void _handlePressed() {
            if (this._haveTriggeredAction) {
                return;
            }

            this.setState(() => { this._haveTriggeredAction = true; });

            this.widget.onPressed();
            Scaffold.of(this.context).hideCurrentSnackBar(reason: SnackBarClosedReason.action);
        }

        public override Widget build(BuildContext context) {
            return new FlatButton(
                onPressed: this._haveTriggeredAction ? (VoidCallback) null : this._handlePressed,
                child: new Text(this.widget.label),
                textColor: this.widget.textColor,
                disabledTextColor: this.widget.disabledTextColor
            );
        }
    }

    public class SnackBar : StatelessWidget {
        public SnackBar(
            Key key = null,
            Widget content = null,
            Color backgroundColor = null,
            SnackBarAction action = null,
            TimeSpan? duration = null,
            Animation<float> animation = null
        ) : base(key: key) {
            duration = duration ?? SnackBarUtils._kSnackBarDisplayDuration;
            D.assert(content != null);
            this.content = content;
            this.backgroundColor = backgroundColor;
            this.action = action;
            this.duration = duration.Value;
            this.animation = animation;
        }

        public readonly Widget content;

        public readonly Color backgroundColor;

        public readonly SnackBarAction action;

        public readonly TimeSpan duration;

        public readonly Animation<float> animation;


        public override Widget build(BuildContext context) {
            MediaQueryData mediaQueryData = MediaQuery.of(context);
            D.assert(this.animation != null);

            ThemeData theme = Theme.of(context);
            ThemeData darkTheme = new ThemeData(
                brightness: Brightness.dark,
                accentColor: theme.accentColor,
                accentColorBrightness: theme.accentColorBrightness
            );

            List<Widget> children = new List<Widget> {
                new SizedBox(width: SnackBarUtils._kSnackBarPadding),
                new Expanded(
                    child: new Container(
                        padding: EdgeInsets.symmetric(vertical: SnackBarUtils._kSingleLineVerticalPadding),
                        child: new DefaultTextStyle(
                            style: darkTheme.textTheme.subhead,
                            child: this.content)
                    )
                )
            };

            if (this.action != null) {
                children.Add(ButtonTheme.bar(
                    padding: EdgeInsets.symmetric(horizontal: SnackBarUtils._kSnackBarPadding),
                    textTheme: ButtonTextTheme.accent,
                    child: this.action
                ));
            }
            else {
                children.Add(new SizedBox(width: SnackBarUtils._kSnackBarPadding));
            }

            CurvedAnimation heightAnimation =
                new CurvedAnimation(parent: this.animation, curve: SnackBarUtils._snackBarHeightCurve);
            CurvedAnimation fadeAnimation = new CurvedAnimation(parent: this.animation,
                curve: SnackBarUtils._snackBarFadeCurve, reverseCurve: new Threshold(0.0f));
            Widget snackbar = new SafeArea(
                top: false,
                child: new Row(
                    children: children,
                    crossAxisAlignment: CrossAxisAlignment.center
                )
            );

            snackbar = new Dismissible(
                key: Key.key("dismissible"),
                direction: DismissDirection.down,
                resizeDuration: null,
                onDismissed: (DismissDirection? direction) => {
                    Scaffold.of(context).removeCurrentSnackBar(reason: SnackBarClosedReason.swipe);
                },
                child: new Material(
                    elevation: 6.0f,
                    color: this.backgroundColor ?? SnackBarUtils._kSnackBackground,
                    child: new Theme(
                        data: darkTheme,
                        child: mediaQueryData.accessibleNavigation
                            ? snackbar
                            : new FadeTransition(
                                opacity: fadeAnimation,
                                child: snackbar
                            )
                    )
                )
            );

            return new ClipRect(
                child: mediaQueryData.accessibleNavigation
                    ? snackbar
                    : new AnimatedBuilder(
                        animation: heightAnimation,
                        builder: (BuildContext subContext, Widget child) => {
                            return new Align(
                                alignment: Alignment.topLeft,
                                heightFactor: heightAnimation.value,
                                child: child
                            );
                        },
                        child: snackbar
                    )
            );
        }

        public static AnimationController createAnimationController(TickerProvider vsync) {
            return new AnimationController(
                duration: SnackBarUtils._kSnackBarTransitionDuration,
                debugLabel: "SnackBar",
                vsync: vsync
            );
        }

        public SnackBar withAnimation(Animation<float> newAnimation, Key fallbackKey = null) {
            return new SnackBar(
                key: this.key ?? fallbackKey,
                content: this.content,
                backgroundColor: this.backgroundColor,
                action: this.action,
                duration: this.duration,
                animation: newAnimation
            );
        }
    }
}