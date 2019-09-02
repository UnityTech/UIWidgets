using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public static partial class PopupMenuUtils {
        internal static readonly TimeSpan _kMenuDuration = new TimeSpan(0, 0, 0, 0, 300);
        internal const float _kBaselineOffsetFromBottom = 20.0f;
        internal const float _kMenuCloseIntervalEnd = 2.0f / 3.0f;
        internal const float _kMenuHorizontalPadding = 16.0f;
        internal const float _kMenuItemHeight = 48.0f;
        internal const float _kMenuDividerHeight = 16.0f;
        internal const float _kMenuMaxWidth = 5.0f * _kMenuWidthStep;
        internal const float _kMenuMinWidth = 2.0f * _kMenuWidthStep;
        internal const float _kMenuVerticalPadding = 8.0f;
        internal const float _kMenuWidthStep = 56.0f;
        internal const float _kMenuScreenPadding = 8.0f;
    }

    public abstract class PopupMenuEntry<T> : StatefulWidget {
        protected PopupMenuEntry(Key key = null) : base(key: key) {
        }

        public abstract float height { get; }

        public abstract bool represents(T value);
    }


    public class PopupMenuDivider : PopupMenuEntry<object> {
        public PopupMenuDivider(Key key = null, float height = PopupMenuUtils._kMenuDividerHeight) : base(key: key) {
            this._height = height;
        }

        readonly float _height;

        public override float height {
            get { return this._height; }
        }

        public override bool represents(object value) {
            return false;
        }

        public override State createState() {
            return new _PopupMenuDividerState();
        }
    }

    class _PopupMenuDividerState : State<PopupMenuDivider> {
        public override Widget build(BuildContext context) {
            return new Divider(height: this.widget.height);
        }
    }

    public class PopupMenuItem<T> : PopupMenuEntry<T> {
        public PopupMenuItem(
            Key key = null,
            T value = default,
            bool enabled = true,
            float height = PopupMenuUtils._kMenuItemHeight,
            Widget child = null
        ) : base(key: key) {
            this.value = value;
            this.enabled = enabled;
            this._height = height;
            this.child = child;
        }

        public readonly T value;

        public readonly bool enabled;

        readonly float _height;

        public override float height {
            get { return this._height; }
        }

        public readonly Widget child;

        public override bool represents(T value) {
            return Equals(value, this.value);
        }

        public override State createState() {
            return new PopupMenuItemState<T, PopupMenuItem<T>>();
        }
    }

    public class PopupMenuItemState<T, W> : State<W> where W : PopupMenuItem<T> {
        protected virtual Widget buildChild() {
            return this.widget.child;
        }

        protected virtual void handleTap() {
            Navigator.pop(this.context, this.widget.value);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            TextStyle style = theme.textTheme.subhead;
            if (!this.widget.enabled) {
                style = style.copyWith(color: theme.disabledColor);
            }

            Widget item = new AnimatedDefaultTextStyle(
                style: style,
                duration: Constants.kThemeChangeDuration,
                child: new Baseline(
                    baseline: this.widget.height - PopupMenuUtils._kBaselineOffsetFromBottom,
                    baselineType: style.textBaseline,
                    child: this.buildChild()
                )
            );

            if (!this.widget.enabled) {
                bool isDark = theme.brightness == Brightness.dark;
                item = IconTheme.merge(
                    data: new IconThemeData(opacity: isDark ? 0.5f : 0.38f),
                    child: item
                );
            }

            return new InkWell(
                onTap: this.widget.enabled ? this.handleTap : (GestureTapCallback) null,
                child: new Container(
                    height: this.widget.height,
                    padding: EdgeInsets.symmetric(horizontal: PopupMenuUtils._kMenuHorizontalPadding),
                    child: item
                )
            );
        }
    }

    public class PopupMenuItemSingleTickerProviderState<T, W> : SingleTickerProviderStateMixin<W>
        where W : PopupMenuItem<T> {
        protected virtual Widget buildChild() {
            return this.widget.child;
        }

        protected virtual void handleTap() {
            Navigator.pop(this.context, this.widget.value);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            TextStyle style = theme.textTheme.subhead;
            if (!this.widget.enabled) {
                style = style.copyWith(color: theme.disabledColor);
            }

            Widget item = new AnimatedDefaultTextStyle(
                style: style,
                duration: Constants.kThemeChangeDuration,
                child: new Baseline(
                    baseline: this.widget.height - PopupMenuUtils._kBaselineOffsetFromBottom,
                    baselineType: style.textBaseline,
                    child: this.buildChild()
                )
            );

            if (!this.widget.enabled) {
                bool isDark = theme.brightness == Brightness.dark;
                item = IconTheme.merge(
                    data: new IconThemeData(opacity: isDark ? 0.5f : 0.38f),
                    child: item
                );
            }

            return new InkWell(
                onTap: this.widget.enabled ? this.handleTap : (GestureTapCallback) null,
                child: new Container(
                    height: this.widget.height,
                    padding: EdgeInsets.symmetric(horizontal: PopupMenuUtils._kMenuHorizontalPadding),
                    child: item
                )
            );
        }
    }

    class CheckedPopupMenuItem<T> : PopupMenuItem<T> {
        public CheckedPopupMenuItem(
            Key key = null,
            T value = default,
            bool isChecked = false,
            bool enabled = true,
            Widget child = null
        ) : base(
            key: key,
            value: value,
            enabled: enabled,
            child: child
        ) {
            this.isChecked = isChecked;
        }

        public readonly bool isChecked;

        public override State createState() {
            return new _CheckedPopupMenuItemState<T>();
        }
    }

    class _CheckedPopupMenuItemState<T> : PopupMenuItemSingleTickerProviderState<T, CheckedPopupMenuItem<T>> {
        static readonly TimeSpan _fadeDuration = new TimeSpan(0, 0, 0, 0, 150);

        AnimationController _controller;

        Animation<float> _opacity {
            get { return this._controller.view; }
        }

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: _fadeDuration, vsync: this);
            this._controller.setValue(this.widget.isChecked ? 1.0f : 0.0f);
            this._controller.addListener(() => this.setState(() => {
                /* animation changed */
            }));
        }

        protected override void handleTap() {
            if (this.widget.isChecked) {
                this._controller.reverse();
            }
            else {
                this._controller.forward();
            }

            base.handleTap();
        }

        protected override Widget buildChild() {
            return new ListTile(
                enabled: this.widget.enabled,
                leading: new FadeTransition(
                    opacity: this._opacity,
                    child: new Icon(this._controller.isDismissed ? null : Icons.done)
                ),
                title: this.widget.child
            );
        }
    }

    class _PopupMenu<T> : StatelessWidget {
        public _PopupMenu(
            Key key = null,
            _PopupMenuRoute<T> route = null
        ) : base(key: key) {
            this.route = route;
        }

        public readonly _PopupMenuRoute<T> route;

        public override Widget build(BuildContext context) {
            float unit = 1.0f / (this.route.items.Count + 1.5f);
            List<Widget> children = new List<Widget>();

            for (int i = 0; i < this.route.items.Count; i += 1) {
                float start = (i + 1) * unit;
                float end = (start + 1.5f * unit).clamp(0.0f, 1.0f);
                Widget item = this.route.items[i];
                if (this.route.initialValue != null && this.route.items[i].represents((T) this.route.initialValue)) {
                    item = new Container(
                        color: Theme.of(context).highlightColor,
                        child: item
                    );
                }

                children.Add(new FadeTransition(
                    opacity: new CurvedAnimation(
                        parent: this.route.animation,
                        curve: new Interval(start, end)
                    ),
                    child: item
                ));
            }

            CurveTween opacity = new CurveTween(curve: new Interval(0.0f, 1.0f / 3.0f));
            CurveTween width = new CurveTween(curve: new Interval(0.0f, unit));
            CurveTween height = new CurveTween(curve: new Interval(0.0f, unit * this.route.items.Count));

            Widget child = new ConstrainedBox(
                constraints: new BoxConstraints(
                    minWidth: PopupMenuUtils._kMenuMinWidth,
                    maxWidth: PopupMenuUtils._kMenuMaxWidth
                ),
                child: new IntrinsicWidth(
                    stepWidth: PopupMenuUtils._kMenuWidthStep,
                    child: new SingleChildScrollView(
                        padding: EdgeInsets.symmetric(
                            vertical: PopupMenuUtils._kMenuVerticalPadding
                        ),
                        child: new ListBody(children: children)
                    )
                )
            );

            return new AnimatedBuilder(
                animation: this.route.animation,
                builder: (_, builderChild) => {
                    return new Opacity(
                        opacity: opacity.evaluate(this.route.animation),
                        child: new Material(
                            type: MaterialType.card,
                            elevation: this.route.elevation,
                            child: new Align(
                                alignment: Alignment.topRight,
                                widthFactor: width.evaluate(this.route.animation),
                                heightFactor: height.evaluate(this.route.animation),
                                child: builderChild
                            )
                        )
                    );
                },
                child: child
            );
        }
    }

    class _PopupMenuRouteLayout : SingleChildLayoutDelegate {
        public _PopupMenuRouteLayout(RelativeRect position, float? selectedItemOffset) {
            this.position = position;
            this.selectedItemOffset = selectedItemOffset;
        }

        public readonly RelativeRect position;

        public readonly float? selectedItemOffset;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return BoxConstraints.loose(constraints.biggest -
                                        new Offset(
                                            PopupMenuUtils._kMenuScreenPadding * 2.0f,
                                            PopupMenuUtils._kMenuScreenPadding * 2.0f));
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            float y;
            if (this.selectedItemOffset == null) {
                y = this.position.top;
            }
            else {
                y = this.position.top + (size.height - this.position.top - this.position.bottom) / 2.0f -
                    this.selectedItemOffset.Value;
            }

            float x;
            if (this.position.left > this.position.right) {
                x = size.width - this.position.right - childSize.width;
            }
            else if (this.position.left < this.position.right) {
                x = this.position.left;
            }
            else {
                x = this.position.left;
            }

            if (x < PopupMenuUtils._kMenuScreenPadding) {
                x = PopupMenuUtils._kMenuScreenPadding;
            }
            else if (x + childSize.width > size.width - PopupMenuUtils._kMenuScreenPadding) {
                x = size.width - childSize.width - PopupMenuUtils._kMenuScreenPadding;
            }

            if (y < PopupMenuUtils._kMenuScreenPadding) {
                y = PopupMenuUtils._kMenuScreenPadding;
            }
            else if (y + childSize.height > size.height - PopupMenuUtils._kMenuScreenPadding) {
                y = size.height - childSize.height - PopupMenuUtils._kMenuScreenPadding;
            }

            return new Offset(x, y);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            return this.position != ((_PopupMenuRouteLayout) oldDelegate).position;
        }
    }

    class _PopupMenuRoute<T> : PopupRoute {
        public _PopupMenuRoute(
            RelativeRect position = null,
            List<PopupMenuEntry<T>> items = null,
            object initialValue = null,
            float elevation = 8.0f,
            ThemeData theme = null
        ) {
            this.position = position;
            this.items = items;
            this.initialValue = initialValue;
            this.elevation = elevation;
            this.theme = theme;
        }

        public readonly RelativeRect position;
        public readonly List<PopupMenuEntry<T>> items;
        public readonly object initialValue;
        public readonly float elevation;
        public readonly ThemeData theme;

        public override Animation<float> createAnimation() {
            return new CurvedAnimation(
                parent: base.createAnimation(),
                curve: Curves.linear,
                reverseCurve: new Interval(0.0f, PopupMenuUtils._kMenuCloseIntervalEnd)
            );
        }

        public override TimeSpan transitionDuration {
            get { return PopupMenuUtils._kMenuDuration; }
        }

        public override bool barrierDismissible {
            get { return true; }
        }

        public override Color barrierColor {
            get { return null; }
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            float? selectedItemOffset = null;
            if (this.initialValue != null) {
                float y = PopupMenuUtils._kMenuVerticalPadding;
                foreach (PopupMenuEntry<T> entry in this.items) {
                    if (entry.represents((T) this.initialValue)) {
                        selectedItemOffset = y + entry.height / 2.0f;
                        break;
                    }

                    y += entry.height;
                }
            }

            Widget menu = new _PopupMenu<T>(route: this);
            if (this.theme != null) {
                menu = new Theme(data: this.theme, child: menu);
            }

            return MediaQuery.removePadding(
                context: context,
                removeTop: true,
                removeBottom: true,
                removeLeft: true,
                removeRight: true,
                child: new Builder(
                    builder: _ => new CustomSingleChildLayout(
                        layoutDelegate: new _PopupMenuRouteLayout(
                            this.position,
                            selectedItemOffset
                        ),
                        child: menu
                    ))
            );
        }
    }

    public static partial class PopupMenuUtils {
        public static IPromise<object> showMenu<T>(
            BuildContext context,
            RelativeRect position,
            List<PopupMenuEntry<T>> items,
            T initialValue,
            float elevation = 8.0f
        ) {
            D.assert(context != null);
            D.assert(position != null);
            D.assert(items != null && items.isNotEmpty());
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            return Navigator.push(context, new _PopupMenuRoute<T>(
                position: position,
                items: items,
                initialValue: initialValue,
                elevation: elevation,
                theme: Theme.of(context, shadowThemeOnly: true)
            ));
        }
    }

    public delegate void PopupMenuItemSelected<T>(T value);

    public delegate void PopupMenuCanceled();

    public delegate List<PopupMenuEntry<T>> PopupMenuItemBuilder<T>(BuildContext context);

    public class PopupMenuButton<T> : StatefulWidget {
        public PopupMenuButton(
            Key key = null,
            PopupMenuItemBuilder<T> itemBuilder = null,
            T initialValue = default,
            PopupMenuItemSelected<T> onSelected = null,
            PopupMenuCanceled onCanceled = null,
            string tooltip = null,
            float elevation = 8.0f,
            EdgeInsets padding = null,
            Widget child = null,
            Icon icon = null,
            Offset offset = null
        ) : base(key: key) {
            offset = offset ?? Offset.zero;
            D.assert(itemBuilder != null);
            D.assert(offset != null);
            D.assert(!(child != null && icon != null));

            this.itemBuilder = itemBuilder;
            this.initialValue = initialValue;
            this.onSelected = onSelected;
            this.onCanceled = onCanceled;
            this.tooltip = tooltip;
            this.elevation = elevation;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.child = child;
            this.icon = icon;
            this.offset = offset;
        }


        public readonly PopupMenuItemBuilder<T> itemBuilder;

        public readonly T initialValue;

        public readonly PopupMenuItemSelected<T> onSelected;

        public readonly PopupMenuCanceled onCanceled;

        public readonly string tooltip;

        public readonly float elevation;

        public readonly EdgeInsets padding;

        public readonly Widget child;

        public readonly Icon icon;

        public readonly Offset offset;

        public override State createState() {
            return new _PopupMenuButtonState<T>();
        }
    }

    class _PopupMenuButtonState<T> : State<PopupMenuButton<T>> {
        void showButtonMenu() {
            RenderBox button = (RenderBox) this.context.findRenderObject();
            RenderBox overlay = (RenderBox) Overlay.of(this.context).context.findRenderObject();
            RelativeRect position = RelativeRect.fromRect(
                Rect.fromPoints(
                    button.localToGlobal(this.widget.offset, ancestor: overlay),
                    button.localToGlobal(button.size.bottomRight(Offset.zero), ancestor: overlay)
                ),
                Offset.zero & overlay.size
            );
            PopupMenuUtils.showMenu(
                    context: this.context,
                    elevation: this.widget.elevation,
                    items: this.widget.itemBuilder(this.context),
                    initialValue: this.widget.initialValue,
                    position: position
                )
                .Then(newValue => {
                    if (!this.mounted) {
                        return;
                    }

                    if (newValue == null) {
                        if (this.widget.onCanceled != null) {
                            this.widget.onCanceled();
                        }

                        return;
                    }

                    if (this.widget.onSelected != null) {
                        this.widget.onSelected((T) newValue);
                    }
                });
        }

        Icon _getIcon(RuntimePlatform platform) {
            switch (platform) {
                case RuntimePlatform.IPhonePlayer:
                    return new Icon(Icons.more_horiz);
                default:
                    return new Icon(Icons.more_vert);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            return this.widget.child != null
                ? (Widget) new InkWell(
                    onTap: this.showButtonMenu,
                    child: this.widget.child
                )
                : new IconButton(
                    icon: this.widget.icon ?? this._getIcon(Theme.of(context).platform),
                    padding: this.widget.padding,
                    tooltip: this.widget.tooltip ?? MaterialLocalizations.of(context).showMenuTooltip,
                    onPressed: this.showButtonMenu
                );
        }
    }
}