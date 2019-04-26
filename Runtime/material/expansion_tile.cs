using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class ExpansionTileUtils {
        public static readonly TimeSpan _kExpand = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class ExpansionTile : StatefulWidget {
        public ExpansionTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Color backgroundColor = null,
            ValueChanged<bool> onExpansionChanged = null,
            List<Widget> children = null,
            Widget trailing = null,
            bool initiallyExpanded = false
        ) : base(key: key) {
            D.assert(title != null);
            this.leading = leading;
            this.title = title;
            this.backgroundColor = backgroundColor;
            this.onExpansionChanged = onExpansionChanged;
            this.children = children ?? new List<Widget>();
            this.trailing = trailing;
            this.initiallyExpanded = initiallyExpanded;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly ValueChanged<bool> onExpansionChanged;

        public readonly List<Widget> children;

        public readonly Color backgroundColor;

        public readonly Widget trailing;

        public readonly bool initiallyExpanded;

        public override State createState() {
            return new _ExpansionTileState();
        }
    }

    public class _ExpansionTileState : SingleTickerProviderStateMixin<ExpansionTile> {
        static readonly Animatable<float> _easeOutTween = new CurveTween(curve: Curves.easeOut);
        static readonly Animatable<float> _easeInTween = new CurveTween(curve: Curves.easeIn);
        static readonly Animatable<float> _halfTween = new FloatTween(begin: 0.0f, end: 0.5f);

        readonly ColorTween _borderColorTween = new ColorTween();
        readonly ColorTween _headerColorTween = new ColorTween();
        readonly ColorTween _iconColorTween = new ColorTween();
        readonly ColorTween _backgroundColorTween = new ColorTween();

        AnimationController _controller;
        Animation<float> _iconTurns;
        Animation<float> _heightFactor;
        Animation<Color> _borderColor;
        Animation<Color> _headerColor;
        Animation<Color> _iconColor;
        Animation<Color> _backgroundColor;

        bool _isExpanded = false;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: ExpansionTileUtils._kExpand, vsync: this);
            this._heightFactor = this._controller.drive(_easeInTween);
            this._iconTurns = this._controller.drive(_halfTween.chain(_easeInTween));
            this._borderColor = this._controller.drive(this._borderColorTween.chain(_easeOutTween));
            this._headerColor = this._controller.drive(this._headerColorTween.chain(_easeInTween));
            this._iconColor = this._controller.drive(this._iconColorTween.chain(_easeInTween));
            this._backgroundColor = this._controller.drive(this._backgroundColorTween.chain(_easeOutTween));

            this._isExpanded = PageStorage.of(this.context)?.readState(this.context) == null
                ? this.widget.initiallyExpanded
                : (bool) PageStorage.of(this.context)?.readState(this.context);

            if (this._isExpanded) {
                this._controller.setValue(1.0f);
            }
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        void _handleTap() {
            this.setState(() => {
                this._isExpanded = !this._isExpanded;
                if (this._isExpanded) {
                    this._controller.forward();
                }
                else {
                    this._controller.reverse().Then(() => {
                        if (!this.mounted) {
                            return;
                        }

                        this.setState(() => { });
                    });
                }

                PageStorage.of(this.context)?.writeState(this.context, this._isExpanded);
            });
            if (this.widget.onExpansionChanged != null) {
                this.widget.onExpansionChanged(this._isExpanded);
            }
        }

        Widget _buildChildren(BuildContext context, Widget child) {
            Color borderSideColor = this._borderColor.value ?? Colors.transparent;

            return new Container(
                decoration: new BoxDecoration(
                    color: this._backgroundColor.value ?? Colors.transparent,
                    border: new Border(
                        top: new BorderSide(color: borderSideColor),
                        bottom: new BorderSide(color: borderSideColor))),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        ListTileTheme.merge(
                            iconColor: this._iconColor.value,
                            textColor: this._headerColor.value,
                            child: new ListTile(
                                onTap: this._handleTap,
                                leading: this.widget.leading,
                                title: this.widget.title,
                                trailing: this.widget.trailing ?? new RotationTransition(
                                              turns: this._iconTurns,
                                              child: new Icon(Icons.expand_more)
                                          )
                            )
                        ),
                        new ClipRect(
                            child: new Align(
                                heightFactor: this._heightFactor.value,
                                child: child)
                        )
                    }
                )
            );
        }

        public override void didChangeDependencies() {
            ThemeData theme = Theme.of(this.context);
            this._borderColorTween.end = theme.dividerColor;
            this._headerColorTween.begin = theme.textTheme.subhead.color;
            this._headerColorTween.end = theme.accentColor;
            this._iconColorTween.begin = theme.unselectedWidgetColor;
            this._iconColorTween.end = theme.accentColor;
            this._backgroundColorTween.end = this.widget.backgroundColor;
            base.didChangeDependencies();
        }

        public override Widget build(BuildContext context) {
            bool closed = !this._isExpanded && this._controller.isDismissed;
            return new AnimatedBuilder(
                animation: this._controller.view,
                builder: this._buildChildren,
                child: closed ? null : new Column(children: this.widget.children));
        }
    }
}