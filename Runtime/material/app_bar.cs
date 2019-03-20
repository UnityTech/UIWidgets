using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class AppBarUtils {
        internal const float _kLeadingWidth = Constants.kToolbarHeight;
    }

    class _ToolbarContainerLayout : SingleChildLayoutDelegate {
        public _ToolbarContainerLayout() {
        }

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.tighten(height: Constants.kToolbarHeight);
        }

        public override Size getSize(BoxConstraints constraints) {
            return new Size(constraints.maxWidth, Constants.kToolbarHeight);
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return new Offset(0.0f, size.height - childSize.height);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            return false;
        }
    }

    public class AppBar : PreferredSizeWidget {
        public AppBar(
            Key key = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            Widget title = null,
            List<Widget> actions = null,
            Widget flexibleSpace = null,
            PreferredSizeWidget bottom = null,
            float elevation = 4.0f,
            Color backgroundColor = null,
            Brightness? brightness = null,
            IconThemeData iconTheme = null,
            TextTheme textTheme = null,
            bool primary = true,
            bool? centerTitle = null,
            float titleSpacing = NavigationToolbar.kMiddleSpacing,
            float toolbarOpacity = 1.0f,
            float bottomOpacity = 1.0f
        ) : base(key: key) {
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.title = title;
            this.actions = actions;
            this.flexibleSpace = flexibleSpace;
            this.bottom = bottom;
            this.elevation = elevation;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.iconTheme = iconTheme;
            this.textTheme = textTheme;
            this.primary = primary;
            this.centerTitle = centerTitle;
            this.titleSpacing = titleSpacing;
            this.toolbarOpacity = toolbarOpacity;
            this.bottomOpacity = bottomOpacity;
            this.preferredSize = Size.fromHeight(Constants.kToolbarHeight + (bottom?.preferredSize?.height ?? 0.0f));
        }

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly Widget title;

        public readonly List<Widget> actions;

        public readonly Widget flexibleSpace;

        public readonly PreferredSizeWidget bottom;

        public readonly float elevation;

        public readonly Color backgroundColor;

        public readonly Brightness? brightness;

        public readonly IconThemeData iconTheme;

        public readonly TextTheme textTheme;

        public readonly bool primary;

        public readonly bool? centerTitle;

        public readonly float titleSpacing;

        public readonly float toolbarOpacity;

        public readonly float bottomOpacity;

        public override Size preferredSize { get; }

        public bool? _getEffectiveCenterTitle(ThemeData themeData) {
            if (this.centerTitle != null) {
                return this.centerTitle;
            }

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                    return this.actions == null || this.actions.Count < 2;
                default:
                    return false;
            }

            return null;
        }

        public override State createState() {
            return new _AppBarState();
        }
    }


    class _AppBarState : State<AppBar> {
        void _handleDrawerButton() {
            Scaffold.of(this.context).openDrawer();
        }

        void _handleDrawerButtonEnd() {
            Scaffold.of(this.context).openEndDrawer();
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            ThemeData themeData = Theme.of(context);
            ScaffoldState scaffold = Scaffold.of(context, nullOk: true);
            ModalRoute parentRoute = ModalRoute.of(context);

            bool hasDrawer = scaffold?.hasDrawer ?? false;
            bool hasEndDrawer = scaffold?.hasEndDrawer ?? false;
            bool canPop = parentRoute?.canPop ?? false;
            bool useCloseButton = parentRoute is PageRoute && ((PageRoute) parentRoute).fullscreenDialog;

            IconThemeData appBarIconTheme = this.widget.iconTheme ?? themeData.primaryIconTheme;
            TextStyle centerStyle = this.widget.textTheme?.title ?? themeData.primaryTextTheme.title;
            TextStyle sideStyle = this.widget.textTheme?.body1 ?? themeData.primaryTextTheme.body1;

            if (this.widget.toolbarOpacity != 1.0f) {
                float opacity =
                    new Interval(0.25f, 1.0f, curve: Curves.fastOutSlowIn).transform(this.widget.toolbarOpacity);
                if (centerStyle?.color != null) {
                    centerStyle = centerStyle.copyWith(color: centerStyle.color.withOpacity(opacity));
                }

                if (sideStyle?.color != null) {
                    sideStyle = sideStyle.copyWith(color: sideStyle.color.withOpacity(opacity));
                }

                appBarIconTheme = appBarIconTheme.copyWith(
                    opacity: opacity * (appBarIconTheme.opacity ?? 1.0f)
                );
            }

            Widget leading = this.widget.leading;
            if (leading == null && this.widget.automaticallyImplyLeading) {
                if (hasDrawer) {
                    leading = new IconButton(
                        icon: new Icon(Icons.menu),
                        onPressed: this._handleDrawerButton,
                        tooltip: MaterialLocalizations.of(context).openAppDrawerTooltip);
                }
                else {
                    if (canPop) {
                        leading = useCloseButton ? (Widget) new CloseButton() : new BackButton();
                    }
                }
            }

            if (leading != null) {
                leading = new ConstrainedBox(
                    constraints: BoxConstraints.tightFor(width: AppBarUtils._kLeadingWidth),
                    child: leading);
            }

            Widget title = this.widget.title;
            if (title != null) {
                bool namesRoute = false;
                switch (Application.platform) {
                    case RuntimePlatform.IPhonePlayer:
                        break;
                    default:
                        namesRoute = true;
                        break;
                }

                title = new DefaultTextStyle(
                    style: centerStyle,
                    softWrap: false,
                    overflow: TextOverflow.ellipsis,
                    child: title);
            }

            Widget actions = null;
            if (this.widget.actions != null && this.widget.actions.isNotEmpty()) {
                actions = new Row(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: this.widget.actions);
            }
            else if (hasEndDrawer) {
                actions = new IconButton(
                    icon: new Icon(Icons.menu),
                    onPressed: this._handleDrawerButtonEnd,
                    tooltip: MaterialLocalizations.of(context).openAppDrawerTooltip);
            }

            Widget toolbar = new NavigationToolbar(
                leading: leading,
                middle: title,
                trailing: actions,
                centerMiddle: this.widget._getEffectiveCenterTitle(themeData).Value,
                middleSpacing: this.widget.titleSpacing);

            Widget appBar = new ClipRect(
                child: new CustomSingleChildLayout(
                    layoutDelegate: new _ToolbarContainerLayout(),
                    child: IconTheme.merge(
                        data: appBarIconTheme,
                        child: new DefaultTextStyle(
                            style: sideStyle,
                            child: toolbar)
                    )
                )
            );

            if (this.widget.bottom != null) {
                appBar = new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new Flexible(
                            child: new ConstrainedBox(
                                constraints: new BoxConstraints(maxHeight: Constants.kToolbarHeight),
                                child: appBar
                            )
                        ),
                        this.widget.bottomOpacity == 1.0f
                            ? (Widget) this.widget.bottom
                            : new Opacity(
                                opacity: new Interval(0.25f, 1.0f, curve: Curves.fastOutSlowIn).transform(this.widget
                                    .bottomOpacity),
                                child: this.widget.bottom
                            )
                    }
                );
            }

            if (this.widget.primary) {
                appBar = new SafeArea(
                    top: true,
                    child: appBar);
            }

            appBar = new Align(
                alignment: Alignment.topCenter,
                child: appBar);

            if (this.widget.flexibleSpace != null) {
                appBar = new Stack(
                    fit: StackFit.passthrough,
                    children: new List<Widget> {
                        this.widget.flexibleSpace,
                        appBar
                    }
                );
            }

            Brightness brightness = this.widget.brightness ?? themeData.primaryColorBrightness;
            SystemUiOverlayStyle overlayStyle = brightness == Brightness.dark
                ? SystemUiOverlayStyle.light
                : SystemUiOverlayStyle.dark;

            return new AnnotatedRegion<SystemUiOverlayStyle>(
                value: overlayStyle,
                child: new Material(
                    color: this.widget.backgroundColor ?? themeData.primaryColor,
                    elevation: this.widget.elevation,
                    child: appBar
                ));
        }
    }
}