using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class Dialog : StatelessWidget {
        public Dialog(
            Key key = null,
            Color backgroundColor = null,
            float? elevation = null,
            TimeSpan? insetAnimationDuration = null,
            Curve insetAnimationCurve = null,
            ShapeBorder shape = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.insetAnimationDuration = insetAnimationDuration ?? new TimeSpan(0, 0, 0, 0, 100);
            this.insetAnimationCurve = insetAnimationCurve ?? Curves.decelerate;
            this.shape = shape;
        }

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly TimeSpan insetAnimationDuration;

        public readonly Curve insetAnimationCurve;

        public readonly ShapeBorder shape;

        public readonly Widget child;

        public static readonly RoundedRectangleBorder _defaultDialogShape =
            new RoundedRectangleBorder(borderRadius: BorderRadius.all(Radius.circular(2.0f)));

        const float _defaultElevation = 24.0f;

        public override Widget build(BuildContext context) {
            DialogTheme dialogTheme = DialogTheme.of(context);

            return new AnimatedPadding(
                padding: MediaQuery.of(context).viewInsets + EdgeInsets.symmetric(horizontal: 40.0f, vertical: 24.0f),
                duration: this.insetAnimationDuration,
                curve: this.insetAnimationCurve,
                child: MediaQuery.removeViewInsets(
                    removeLeft: true,
                    removeTop: true,
                    removeRight: true,
                    removeBottom: true,
                    context: context,
                    child: new Center(
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(minWidth: 280.0f),
                            child: new Material(
                                color: this.backgroundColor ?? dialogTheme.backgroundColor ??
                                       Theme.of(context).dialogBackgroundColor,
                                elevation: this.elevation ?? dialogTheme.elevation ?? _defaultElevation,
                                shape: this.shape ?? dialogTheme.shape ?? _defaultDialogShape,
                                type: MaterialType.card,
                                child: this.child
                            )
                        )
                    )
                )
            );
        }
    }

    public class AlertDialog : StatelessWidget {
        public AlertDialog(
            Key key = null,
            Widget title = null,
            EdgeInsets titlePadding = null,
            TextStyle titleTextStyle = null,
            Widget content = null,
            EdgeInsets contentPadding = null,
            TextStyle contentTextStyle = null,
            List<Widget> actions = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null
        ) : base(key: key) {
            this.title = title;
            this.titlePadding = titlePadding;
            this.titleTextStyle = titleTextStyle;
            this.content = content;
            this.contentPadding = contentPadding ?? EdgeInsets.fromLTRB(24.0f, 20.0f, 24.0f, 24.0f);
            this.contentTextStyle = contentTextStyle;
            this.actions = actions;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
        }

        public readonly Widget title;
        public readonly EdgeInsets titlePadding;
        public readonly TextStyle titleTextStyle;
        public readonly Widget content;
        public readonly EdgeInsets contentPadding;
        public readonly TextStyle contentTextStyle;
        public readonly List<Widget> actions;
        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context) {
            // D.assert(debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context);
            DialogTheme dialogTheme = DialogTheme.of(context);

            List<Widget> children = new List<Widget>();

            if (this.title != null) {
                children.Add(new Padding(
                    padding: this.titlePadding ??
                             EdgeInsets.fromLTRB(24.0f, 24.0f, 24.0f, this.content == null ? 20.0f : 0.0f),
                    child: new DefaultTextStyle(
                        style: this.titleTextStyle ?? dialogTheme.titleTextStyle ?? theme.textTheme.title,
                        child: this.title
                    )
                ));
            }

            if (this.content != null) {
                children.Add(new Flexible(
                    child: new Padding(
                        padding: this.contentPadding,
                        child: new DefaultTextStyle(
                            style: this.contentTextStyle ?? dialogTheme.contentTextStyle ?? theme.textTheme.subhead,
                            child: this.content
                        )
                    )
                ));
            }

            if (this.actions != null) {
                children.Add(ButtonTheme.bar(
                    child: new ButtonBar(
                        children: this.actions
                    )
                ));
            }

            Widget dialogChild = new IntrinsicWidth(
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: children
                )
            );

            return new Dialog(
                backgroundColor: this.backgroundColor,
                elevation: this.elevation,
                shape: this.shape,
                child: dialogChild
            );
        }
    }

    public class SimpleDialogOption : StatelessWidget {
        public SimpleDialogOption(
            Key key = null,
            VoidCallback onPressed = null,
            Widget child = null
        ) : base(key: key) {
            this.onPressed = onPressed;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new InkWell(
                onTap: () => this.onPressed(),
                child: new Padding(
                    padding: EdgeInsets.symmetric(vertical: 8.0f, horizontal: 24.0f),
                    child: this.child
                )
            );
        }
    }

    public class SimpleDialog : StatelessWidget {
        public SimpleDialog(
            Key key = null,
            Widget title = null,
            EdgeInsets titlePadding = null,
            List<Widget> children = null,
            EdgeInsets contentPadding = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null
        ) : base(key: key) {
            this.title = title;
            this.titlePadding = titlePadding ?? EdgeInsets.fromLTRB(24.0f, 24.0f, 24.0f, 0.0f);
            this.children = children;
            this.contentPadding = contentPadding ?? EdgeInsets.fromLTRB(0.0f, 12.0f, 0.0f, 16.0f);
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
        }

        public readonly Widget title;

        public readonly EdgeInsets titlePadding;

        public readonly List<Widget> children;

        public readonly EdgeInsets contentPadding;

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            List<Widget> body = new List<Widget>();

            if (this.title != null) {
                body.Add(new Padding(
                    padding: this.titlePadding,
                    child: new DefaultTextStyle(
                        style: Theme.of(context).textTheme.title,
                        child: this.title
                    )
                ));
            }

            if (this.children != null) {
                body.Add(new Flexible(
                    child: new SingleChildScrollView(
                        padding: this.contentPadding,
                        child: new ListBody(children: this.children)
                    )
                ));
            }

            Widget dialogChild = new IntrinsicWidth(
                stepWidth: 56.0f,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(minWidth: 280.0f),
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: body
                    )
                )
            );

            return new Dialog(
                backgroundColor: this.backgroundColor,
                elevation: this.elevation,
                shape: this.shape,
                child: dialogChild
            );
        }
    }

    public static class DialogUtils {
        static Widget _buildMaterialDialogTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return new FadeTransition(
                opacity: new CurvedAnimation(
                    parent: animation,
                    curve: Curves.easeOut
                ),
                child: child
            );
        }

        public static IPromise<object> showDialog(
            BuildContext context = null,
            bool barrierDismissible = true,
            WidgetBuilder builder = null
        ) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context, shadowThemeOnly: true);
            return widgets.DialogUtils.showGeneralDialog(
                context: context,
                pageBuilder: (buildContext, animation, secondaryAnimation) => {
                    Widget pageChild = new Builder(builder: builder);
                    return new SafeArea(
                        child: new Builder(
                            builder: (_) => theme != null
                                ? new Theme(data: theme, child: pageChild)
                                : pageChild)
                    );
                },
                barrierDismissible: barrierDismissible,
                barrierColor: Colors.black54,
                transitionDuration: new TimeSpan(0, 0, 0, 0, 150),
                transitionBuilder: _buildMaterialDialogTransitions
            );
        }
    }
}